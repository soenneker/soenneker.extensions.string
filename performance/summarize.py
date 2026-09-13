"""Select measurements of retained candidates, preserving each originating report."""
from pathlib import Path
import csv

root = Path(__file__).resolve().parent / 'results'
sources = [
    ('final', 'Base64Benchmarks', None),
    ('isolated', 'LoggingBenchmarks', None),
    ('isolated', 'WeightedBenchmarks', None),
    ('final', 'StringBenchmarks', {'OrdinalLower', 'Scriban', 'SplitSingle', 'WhitespaceAscii', 'WhitespaceUnicode'}),
    ('refinement', 'StringBenchmarks', {'InvariantLower'}),
    ('confirmation', 'StringBenchmarks', {'AlphaNumeric', 'Digits', 'HttpLike'}),
    ('slug-final', 'StringBenchmarks', {'SlugClean', 'SlugDirty'}),
    ('slug-final', 'SlugShapeBenchmarks', None),
    ('late-change', 'SlugShapeBenchmarks', None),
    ('final', 'DependencyBenchmarks', {'AsciiEquals', 'Join', 'Sha256Hex', 'SplitRanges'}),
    ('refinement', 'DependencyBenchmarks', {'AsciiSafe', 'Classify'}),
    ('confirmation', 'DependencyBenchmarks', {'AsciiBytes'}),
    ('isolated', 'DependencyBenchmarks', {'SplitTrimmed'}),
]
fields = ['Suite', 'Method', 'Categories', 'Shape', 'Length', 'Count', 'Url', 'Mean', 'Error', 'StdDev', 'Ratio', 'Allocated', 'Source']
with (root / 'accepted.csv').open('w', newline='', encoding='utf-8') as destination:
    writer = csv.DictWriter(destination, fieldnames=fields)
    writer.writeheader()
    for run, suite, categories in sources:
        path = root / run / 'results' / f'{suite}-report.csv'
        with path.open(newline='', encoding='utf-8-sig') as source:
            for row in csv.DictReader(source):
                if run == 'slug-final' and suite == 'SlugShapeBenchmarks' and row.get('Shape') == 'LateChange':
                    continue
                if categories is not None and row['Categories'] not in categories:
                    continue
                if row['Mean'] == 'NA':
                    raise ValueError(f'Incomplete measurement: {path}, {row["Method"]}')
                result = {key: row.get(key, '') for key in fields}
                result['Suite'] = suite
                result['Source'] = path.relative_to(root).as_posix()
                writer.writerow(result)
