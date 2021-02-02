namespace RainbowOF.Tools
{
    public class StringTools
    {
        public string Truncate(string pSource, int length)
        {
            return (pSource.Length > length) ?
                pSource.Substring(0, length) :
                pSource;
        }
        public string MakeAbbriviation(string name)
        {
            string _newName = string.Empty;
            string _vowels = "AEIOUaeiou -+:%$&*@.";  // remove vowels and spaces and symbols
            int i = 0;
            while ((i < name.Length) && (_newName.Length < 9))  // 9 so links to items mode max 10
            {
                if (!_vowels.Contains(name[i]))
                {
                    _newName += name[i].ToString().ToUpper();
                }
                i++;
            }
            return _newName;
        }
        public string StripHTML(string pSource)
        {
            return System.Text.RegularExpressions.Regex.Replace(pSource, @"<(.|\n)*?>", string.Empty);
        }
    }

}
