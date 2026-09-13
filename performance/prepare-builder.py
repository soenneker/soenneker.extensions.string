"""Prepare the pinned post-audit string baseline; never modify production sources."""
from pathlib import Path
import subprocess

audit = Path(__file__).resolve().parent
repo = audit.parent
commit = '99cbac4'
files = subprocess.check_output(['git', '-C', str(repo), 'ls-tree', '-r', '--name-only', commit, 'src'], text=True).splitlines()
for file in files:
    if file.endswith('.cs'):
        source = subprocess.check_output(['git', '-C', str(repo), 'show', f'{commit}:{file}'], text=True, encoding='utf-8')
        source = source.replace('namespace Soenneker.Extensions.String;', 'namespace BuilderSnapshot;')
        target = audit / 'BuilderSnapshot' / Path(file).name
        target.parent.mkdir(exist_ok=True)
        target.write_text(source, encoding='utf-8')

source = (audit / 'BuilderSnapshot/StringExtension.cs').read_text(encoding='utf-8')
def method(name):
    start = source.index('    public static string? ' + name + '(')
    return source[start:source.index('\n    }', start) + 6]

parts = ['using System.Buffers;\nusing Soenneker.Extensions.Char;\nusing Soenneker.Utils.PooledStringBuilders;\nnamespace BuilderSnapshot;\npublic static partial class StringExtension\n{']
for name, first, predicate in [('RemoveNonDigits', 'firstNonDigit', 'c.IsDigitFast()'), ('RemoveWhiteSpace', 'first', '!c.IsWhiteSpaceFast()'), ('RemoveAllChar', 'first', 'c != removeChar')]:
    prelude = method(name).split('        int outLen =')[0]
    for variant in ['Builder', 'Buffer', 'BuilderSpan']:
        body = prelude.replace(name + '(', name + variant + '(')
        if variant == 'Builder':
            body += '        Span<char> initial = stackalloc char[Math.Min(value.Length, 512)];\n        using var builder = value.Length <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(value.Length);\n'
            body += f'        builder.Append(s[..{first}]);\n        for (int i = {first} + 1; i < s.Length; i++)\n        {{\n            char c = s[i];\n            if ({predicate}) builder.Append(c);\n        }}\n        return builder.ToString();\n    }}'
        else:
            if variant == 'Buffer':
                body += '        char[]? rented = null;\n        Span<char> destination = value.Length <= 512 ? stackalloc char[value.Length] : (rented = ArrayPool<char>.Shared.Rent(value.Length));\n        try\n        {\n'
            else:
                body += '        Span<char> initial = stackalloc char[Math.Min(value.Length, 512)];\n        using var builder = value.Length <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(value.Length);\n        Span<char> destination = builder.AppendSpan(value.Length);\n        {\n'
            body += f'            s[..{first}].CopyTo(destination);\n            int written = {first};\n            for (int i = {first} + 1; i < s.Length; i++)\n            {{\n                char c = s[i];\n                if ({predicate}) destination[written++] = c;\n            }}\n'
            if variant == 'Buffer':
                body += '            return new string(destination[..written]);\n        }\n        finally { if (rented is not null) ArrayPool<char>.Shared.Return(rented); }\n    }'
            else:
                body += '            builder.Shrink(value.Length - written);\n            return builder.ToString();\n        }\n    }'
        parts.append(body)

    if name != 'RemoveAllChar':
        for variant in ['BuilderTail', 'BufferTail', 'BuilderGrowTail']:
            body = prelude.replace(name + '(', name + variant + '(')
            body += f'        ReadOnlySpan<char> tail = s[({first} + 1)..];\n        if (tail.IsEmpty) return value[..{first}];\n'
            if variant != 'BufferTail':
                if variant == 'BuilderGrowTail':
                    body += '        Span<char> initial = stackalloc char[Math.Min(tail.Length, 512)];\n        using var builder = new PooledStringBuilder(initial);\n'
                else:
                    body += '        Span<char> initial = stackalloc char[tail.Length <= 512 ? tail.Length : 0];\n        using var builder = tail.Length <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(tail.Length);\n'
                body += f'        foreach (char c in tail)\n            if ({predicate}) builder.Append(c);\n        return string.Concat(s[..{first}], builder.AsSpan());\n    }}'
            else:
                body += '        char[]? rented = null;\n        Span<char> destination = tail.Length <= 512 ? stackalloc char[tail.Length] : (rented = ArrayPool<char>.Shared.Rent(tail.Length));\n        try\n        {\n            int written = 0;\n'
                body += f'            foreach (char c in tail)\n                if ({predicate}) destination[written++] = c;\n            return string.Concat(s[..{first}], destination[..written]);\n        }}\n        finally {{ if (rented is not null) ArrayPool<char>.Shared.Return(rented); }}\n    }}'
            parts.append(body)

slug = method('Slugify').split('        char[]? rented =')[0].replace('Slugify(', 'SlugifyBuilder(')
slug += '''        Span<char> initial = stackalloc char[Math.Min(source.Length, 512)];
        using var builder = source.Length <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(source.Length);
        int written = WriteSlug(source, builder.AppendSpan(source.Length));
        builder.Shrink(source.Length - written);
        if (written == source.Length && builder.AsSpan().SequenceEqual(source)) return value;
        return builder.ToString();
    }'''
parts.append(slug)
handwritten = (audit / 'BuilderCandidates.cs').read_text(encoding='utf-8')
start = handwritten.index('    public static string ScribanBuffer(')
scriban = handwritten[start:handwritten.index('\n    }', start) + 6].replace('ScribanBuffer(', 'ScribanBuilderSpan(')
scriban = scriban.replace('        char[]? rented = null;\n        Span<char> destination = s.Length <= 512 ? stackalloc char[s.Length] : (rented = ArrayPool<char>.Shared.Rent(s.Length));\n        try', '        Span<char> initial = stackalloc char[Math.Min(s.Length, 512)];\n        using var builder = s.Length <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(s.Length);\n        Span<char> destination = builder.AppendSpan(s.Length);')
scriban = scriban.replace('            return destination[..written].SequenceEqual(s) ? input : new string(destination[..written]);', '            builder.Shrink(s.Length - written);\n            return builder.AsSpan().SequenceEqual(s) ? input : builder.ToString();')
scriban = scriban.replace('        finally { if (rented is not null) ArrayPool<char>.Shared.Return(rented); }\n', '')
parts.extend([scriban, '}'])
(audit / 'BuilderSnapshot/GeneratedCandidates.cs').write_text('\n'.join(parts), encoding='utf-8')
print('Prepared string baseline 99cbac4 and filter/slug candidates. Use builder source fa0f48c (optimization 13789a7).')
