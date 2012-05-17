using System;
using System.IO;

namespace USPS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the FULL path to the file: ");
            var file_to_read = Console.ReadLine();
            string line;

            if (file_to_read != null)
            {
                if (File.Exists(file_to_read))
                {
                    StreamReader _reader = new FileInfo(file_to_read).OpenText();
                    StreamWriter _writer = new StreamWriter(file_to_read + ".out");

                    while ((line = _reader.ReadLine()) != null)
                    {
                        string position_field = "00301904795001000000";

                        position_field += line.Substring(15, 5);
                        position_field += line.Substring(200, 4);
                        position_field += line.Substring(169, 2);

                        if (position_field.Length == 31)
                        {
                            _writer.WriteLine(line + position_field + OpenSource.OneCode.Bars(position_field));
                        }
                        else
                        {
                            Console.WriteLine("Fuck!");
                        }

                    }

                    _reader.Close();
                    _writer.Close();
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                }

                
            }
        }
    }
}
