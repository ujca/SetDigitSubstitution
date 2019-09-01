namespace SetDigitSubstitution
{
    public class DataEntry
        {
            public DataEntry(string section, string subsection, string digits)
            {
                Section = section;
                Subsection = subsection;
                Digits = digits;
            }

            public string Section { get; }
            public string Subsection { get; }
            public string Digits { get; }

            public static DataEntry Parse(string line)
            {
                string[] tokens = line.Split(';');
                return new DataEntry(tokens[1], tokens[2], tokens[0]);
            }
        }
}
