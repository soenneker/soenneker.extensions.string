using New = Soenneker.Extensions.String.StringExtension;
using Old = Baseline.Soenneker.Extensions.String.StringExtension;
using NewSpan = Soenneker.Extensions.Spans.Readonly.Chars.ReadOnlySpanCharExtension;
using OldSpan = Baseline.Soenneker.Extensions.Spans.Readonly.Chars.ReadOnlySpanCharExtension;

internal static class Verification
{
    public static void Run()
    {
        int checks=0;
        void Equal<T>(T actual,T expected,string label)
        {
            checks++;
            if (!EqualityComparer<T>.Default.Equals(actual,expected)) throw new Exception(label);
        }
        var random=new Random(7919);
        const string alphabet="abcXYZ09 -_.,;:\t\r\n\u0085\u00a0\u2000\u2028\u3000éÉǅǆ\ud800\udc00{\"}\\";
        for(int sample=0;sample<10000;sample++)
        {
            int length=sample<100?sample:random.Next(0,1500);
            string value=string.Create(length,random,static(dst,r)=>{for(int i=0;i<dst.Length;i++) dst[i]=alphabet[r.Next(alphabet.Length)];});
            Equal(New.Slugify(value),Old.Slugify(value),"Slugify");
            Equal(New.RemoveWhiteSpace(value),Old.RemoveWhiteSpace(value),"RemoveWhiteSpace");
            Equal(New.ToDashesFromWhiteSpace(value),Old.ToDashesFromWhiteSpace(value),"Dashes");
            Equal(New.ToLowerOrdinal(value),Old.ToLowerOrdinal(value),"LowerOrdinal");
            Equal(New.ToUpperOrdinal(value),Old.ToUpperOrdinal(value),"UpperOrdinal");
            Equal(New.ToLowerInvariantFast(value),Old.ToLowerInvariantFast(value),"LowerInvariant");
            Equal(New.ToUpperInvariantFast(value),Old.ToUpperInvariantFast(value),"UpperInvariant");
            Equal(New.ToEscapedForScriban(value),Old.ToEscapedForScriban(value),"Scriban");
            Equal(New.RemoveNonDigits(value),Old.RemoveNonDigits(value),"RemoveNonDigits");
            Equal(New.IsAlphaNumeric(value),Old.IsAlphaNumeric(value),"IsAlphaNumeric");
            Equal(New.IsHttpUriLike("https://"+value),Old.IsHttpUriLike("https://"+value),"IsHttpUriLike");
            Equal(string.Join('|',New.SplitTrimmedNonEmpty(value,',')??[]),string.Join('|',Old.SplitTrimmedNonEmpty(value,',')??[]),"Split");
            Range[] a=new Range[16], b=new Range[16];
            int an=NewSpan.SplitCommaRanges(value,a), bn=OldSpan.SplitCommaRanges(value,b);
            Equal(an,bn,"RangeCount");
            Equal(a.AsSpan(0,an).SequenceEqual(b.AsSpan(0,bn)),true,"Ranges");
            Equal(NewSpan.JoinCommaSeparated(value,a,0,an),OldSpan.JoinCommaSeparated(value,b,0,bn),"Join");
        }
        for(int i=0;i<=char.MaxValue;i++)
        {
            string value=((char)i).ToString();
            Equal(New.RemoveWhiteSpace(value),char.IsWhiteSpace((char)i)?"":value,"Unicode whitespace "+i);
            Equal(New.RemoveNonDigits(value),char.IsDigit((char)i)?value:"","Unicode digit "+i);
            Equal(New.IsAlphaNumeric(value),char.IsLetterOrDigit((char)i),"Unicode alphanumeric "+i);
            Equal(New.Slugify(value),Old.Slugify(value),"Unicode slug "+i);
        }
        foreach(string value in new[]{"plain", "a-slug_with-separators",new string('a',4096)})
            Equal(ReferenceEquals(New.Slugify(value),value),true,"Slug identity");
        foreach (int length in new[] { 15, 16, 17, 31, 32, 33, 127, 128, 129, 511, 512, 513, 4096 })
        {
            string plain = new string('a', length);
            foreach (string value in new[] { " Hello___WORLD!! " + plain, plain + "!", "_" + plain + "__", new string('A', length), plain + " é " + plain })
                Equal(New.Slugify(value), Old.Slugify(value), "Long slug runs");
        }
        for(int i=0;i<2000;i++)
        {
            byte[] bytes=new byte[random.Next(0,4096)];
            random.NextBytes(bytes);
            Equal(Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension.Classify(bytes),Baseline.Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension.Classify(bytes),"Content classification");
            string standard=Convert.ToBase64String(bytes);
            string url=standard.Replace('+','-').Replace('/','_').TrimEnd('=');
            foreach(string input in new[]{standard,url," \r\n"+standard+"\t",url+"=",url+"?"})
            {
                static string Decode(Func<string,string> decode,string value)
                {
                    try { return "ok:"+decode(value); }
                    catch(FormatException) { return "invalid"; }
                }
                Equal(Decode(New.ToStringFromBase64,input),Decode(Old.ToStringFromBase64,input),"Base64");
            }
            Equal(Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension.ToSha256Hex(bytes),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)),"SHA256");
        }
        for(int i=0;i<10000;i++)
        {
            var bytes=new byte[random.Next(0,1024)];
            for(int j=0;j<bytes.Length;j++) bytes[j]=(byte)random.Next(16,128);
            Equal(Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension.Classify(bytes),Baseline.Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension.Classify(bytes),"Text classification");
            byte[] other=(byte[])bytes.Clone();
            for(int j=0;j<other.Length;j++) if(other[j] is >= 65 and <= 90) other[j]+=32;
            Equal(Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension.Utf8AsciiEqualsIgnoreCase(bytes,other),Baseline.Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension.Utf8AsciiEqualsIgnoreCase(bytes,other),"Byte ASCII equality");
        }
        // Enumerate malformed short inputs as well as ordinary encoded values.
        const string encodedAlphabet="Az09+/-_= \t\r\n!";
        for(int i=0;i<10000;i++)
        {
            string input=string.Create(random.Next(1,24),random,static(dst,r)=>{for(int j=0;j<dst.Length;j++)dst[j]=encodedAlphabet[r.Next(encodedAlphabet.Length)];});
            string? oldResult=null,newResult=null;
            bool oldInvalid=false,newInvalid=false;
            try { oldResult=Old.ToStringFromBase64(input); } catch(FormatException){oldInvalid=true;}
            try { newResult=New.ToStringFromBase64(input); } catch(FormatException){newInvalid=true;}
            Equal(newInvalid,oldInvalid,"Invalid Base64 "+input);
            Equal(newResult,oldResult,"Base64 output");
        }
        Console.WriteLine($"Passed {checks:N0} differential and reference checks.");
    }
}
