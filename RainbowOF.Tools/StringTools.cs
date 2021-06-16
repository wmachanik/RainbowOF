namespace RainbowOF.Tools
{
    public class StringTools
    {
        public string Truncate(string sourceString, int targetLength)
        {
            return (sourceString.Length > targetLength) ?
                sourceString.Substring(0, targetLength) :
                sourceString;
        }
        public string MakeAbbriviation(string priginalName)
        {
            string _newName = string.Empty;
            string _vowels = "AEIOUaeiou -+:%$&*@.";  // remove vowels and spaces and symbols
            int i = 0;
            while ((i < priginalName.Length) && (_newName.Length < 9))  // 9 so links to items mode max 10
            {
                if (!_vowels.Contains(priginalName[i]))
                {
                    _newName += priginalName[i].ToString().ToUpper();
                }
                i++;
            }
            return _newName;
        }
        public string StripHTML(string originalHTML)
        {
            return System.Text.RegularExpressions.Regex.Replace(originalHTML, @"<(.|\n)*?>", string.Empty);
        }
    }

}
