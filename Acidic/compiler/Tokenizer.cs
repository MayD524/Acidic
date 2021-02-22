using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;


namespace Tokenizer
{
    public class Token
    {
        const int EXIT_SUCCESS = 0;
        const int EXIT_FAILURE = 1;

        const int END_OF_FILE = 0x6000;
        // returns a 64 bit key hash
        public static Int16 Get16bithash(string str)
        {
            return Math.Abs((Int16) (str.GetHashCode() & 0xFFFF));
        }
        static readonly Dictionary<string, int> keywords = new Dictionary<string, int>()
        {
            {"int",     0x01},
            {"long",    0x02},
            {"float",   0x03},
            {"bool",    0x04},
            {"str",     0x05},
            {"char",    0x06},
            {"non",     0x10},
            {"func",    0x20},
            {"out",     0x21},
            {"set",     0x25},
            {"goto",    0x26},
            {"return",  0x34},
            {"if",      0xfd},
            {"else",    0xfc},
            {"break",   0xf0}
        };
        static Dictionary<string, Int16> tempMem = new Dictionary<string, Int16>();

        public static void AppendAllBytes(string path, byte[] bytes)
        {
            using(var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        public static string DeclairNumberic(int cmd, string data)
        {
            Int16 cVar = 0;
            // dealing with an int and here we want 0 spaces
            string[] opers = { "+", "-", "*", "/" };
            //bool mutableflag = true;
            string operation = "";

            //if (data.Contains("non")) { mutableflag = false; data = data.Replace("non", ""); }
            data = data.Replace(" ", "");
            if (data.Contains("="))
            {

                string[] tmp = data.Split("=");
                cVar = Get16bithash(tmp[0]);
                string vals = tmp[1];
                tempMem.Add(tmp[0], cVar);
                foreach (string op in opers) { if (vals.Contains(op)) { operation = op; } }
                string[] valSplit = vals.Split(operation);
                for (int x = 0; x < valSplit.Length; x++)
                {
                    if (tempMem.ContainsKey(valSplit[x]))
                    {
                        valSplit[x] = tempMem[valSplit[x]].ToString();
                    }
                }
                string endString = "";
                if (valSplit.Length > 1)
                    endString = $"{cmd} {cVar} {valSplit[0]} {operation} {valSplit[1]};";
                else
                    endString = $"{cmd} {cVar} {valSplit[0]};";
                return endString;
            }
            else
            {
                Console.WriteLine("Not declairing any variable");
                Environment.Exit(EXIT_FAILURE);
                return "";
            }
        }

        public static void TokenRunner(string[] lines, string outFile)
        {
             
            string[] newlines = new string[lines.Length + 2];
            newlines[0] = $"{0x25} {0x4000} {0x01}"; // making the if flag flase
            List<Byte[]> byteArrays = new List<byte[]>();
            string currentline;
            for (int i = 1; i < lines.Length; i++)
            {
                currentline = lines[i];
                string data = "";
                int cmd = 0x00;
                if(currentline.Contains(" "))
                {
                    string[] SplitLine = currentline.Split(" ", 2);
                    data = SplitLine[1];
                    
                    
                    if (keywords.ContainsKey(SplitLine[0]))
                        cmd = keywords[SplitLine[0]];
                    else
                        cmd = 0xFF;
                } else {
                    if(keywords.ContainsKey(currentline))
                    {
                        cmd = keywords[currentline];
                    } else {
                        cmd = 0xFF;
                    }
                }
                

                switch(cmd)
                {
                    case 0x01: // int
                        {
                            newlines[i] = DeclairNumberic(cmd, data);

                            break;
                        }

                    case 0x02: // long
                        {
                            newlines[i] = DeclairNumberic(cmd, data);
                            break;
                        }

                    case 0x03: // float
                        {
                            newlines[i] = DeclairNumberic(cmd, data);
                            break;
                        }

                    case 0x04: // bool
                        {
                            data = data.Replace(" ", ""); // we want 0 spaces in this
                            if (data.Contains("="))
                            {
                                string[] dataSplit = data.Split("=");
                                string endString;
                                bool boolSet = false;

                                Int16 VarHash = Get16bithash(dataSplit[0]);
                                tempMem.Add(dataSplit[0], VarHash);
                                if (tempMem.ContainsKey(dataSplit[1]))
                                {
                                    dataSplit[1] = tempMem[dataSplit[1]].ToString();
                                    endString = $"{cmd} {VarHash} {dataSplit[1]};";
                                }
                                else
                                {
                                    switch (dataSplit[1])
                                    {
                                        case "true": boolSet = true; break;
                                        case "1": boolSet = true; break;
                                        case "false": boolSet = false; break;
                                        case "0": boolSet = false; break;
                                        default: Console.WriteLine("Incorrect cast"); Environment.Exit(EXIT_FAILURE); return;
                                    }
                                    endString = $"{cmd} {VarHash} {boolSet};";

                                }
                                newlines[i] = endString;
                            }
                            else
                            {
                                Console.WriteLine("We are not delairing any variable or has no value.");
                                Environment.Exit(EXIT_FAILURE);
                            }
                            break;
                        }

                    case 0x05: // str
                        if (data.Contains("="))
                        {
                            string[] dataSplit = data.Split("=");
                            string tmpVar = dataSplit[0].Replace(" ", "");
                            Int16 VarHash = Get16bithash(tmpVar);
                            tempMem.Add(tmpVar, VarHash);
                            string strData = dataSplit[1].TrimStart();
                            if (strData.StartsWith("\"") && strData.EndsWith("\""))
                            {
                                strData = strData.Replace("\"", "").Replace(" ", "_"); // replace spaces with _ so that in runtime we can just split by spaces.
                                newlines[i] = $"{cmd} {VarHash} {strData};";
                            } else
                            {
                                Console.WriteLine("Cannot cast string from nonstring");
                                Environment.Exit(EXIT_FAILURE);
                            }
                        }
                        else
                        {
                            Console.WriteLine("We aren't delairing any new variable.");
                            Environment.Exit(EXIT_FAILURE);
                        }
                        break;

                    case 0x21: // out
                        {
                            int mode = 0x00;
                            if (tempMem.ContainsKey(data))
                            {
                                int TempVal = tempMem[data];
                                newlines[i] = $"{cmd} {mode} {TempVal};";
                            }
                            else
                            {
                                mode = 0x01;
                                newlines[i] = $"{cmd} {mode} {data};";
                            }
                            break;
                        }

                    case 0x25: // set
                        if (data.Contains("="))
                        {
                            string[] DataSplit = data.Split("=");
                            if (DataSplit[0].Contains(" ")) { DataSplit[0] = DataSplit[0].Replace(" ", ""); }
                            if (tempMem.ContainsKey(DataSplit[0]))
                            {
                                if (DataSplit[1].StartsWith("\"") && DataSplit[1].EndsWith("\"")) // string type
                                {
                                    if (DataSplit[1].StartsWith(" "))
                                    {
                                        DataSplit[1] = DataSplit[1].TrimStart();
                                    }
                                    DataSplit[1] = DataSplit[1].Replace("\"", "").Replace(" ", "_");
                                    newlines[i] = $"{cmd} {tempMem[DataSplit[0]]} {DataSplit[1]};";
                                } else // any other type
                                {
                                    if (DataSplit[1].Contains(" "))
                                    {
                                        string[] tempArr = DataSplit[1].Split(" ");
                                        for(int x = 0; x < tempArr.Length; x++)
                                        {
                                            string str = tempArr[x];
                                            if (tempMem.ContainsKey(str))
                                            {
                                                tempArr[x] = tempMem[str].ToString();
                                            }
                                        }
                                        string endString = string.Join("", tempArr);
                                        newlines[i] = $"{cmd} {tempMem[DataSplit[0]]} {endString};";
                                    }
                                }
                            } else
                            {
                                Console.WriteLine("Cannont find variable in set. You have to initialize it first.");
                                Environment.Exit(EXIT_FAILURE);
                            }
                        } else
                        {
                            Console.WriteLine("We are net setting any value to a variable.");
                            Environment.Exit(EXIT_FAILURE);
                        }
                        
                        break;
                    
                    case 0xfd: // if
                        Console.WriteLine(data);
                        string[] ops = {"==", "!=", ">", "<", "<=", ">="};
                        string[] dSplit;
                        int checkCode = 0x00;
                        string op = "";
                        foreach(string o in ops)
                        {
                            if(data.Contains(o))
                            {
                                op = o;
                                dSplit = data.Split(o);

                                switch(o)
                                {
                                    case "==": checkCode = 0x40; break;
                                    case "!=": checkCode = 0x41; break;
                                    case ">":  checkCode = 0x42; break;
                                    case "<":  checkCode = 0x43; break;
                                    case "<=": checkCode = 0x44; break;
                                    case ">=": checkCode = 0x45; break;
                                }

                                for(int x = 0; x < dSplit.Length;x++)
                                {
                                    if(tempMem.ContainsKey(dSplit[x]))
                                    {
                                        dSplit[x] = tempMem[dSplit[x]].ToString();
                                    }
                                }
                                newlines[i] = $"{cmd} {dSplit[0]} {checkCode} {dSplit[1]} {0x25} {0x4000} {0x01};";
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(op))
                        {
                            newlines[i] = $"{cmd} {data} {0x40} {0x01} {0x25} {0x4000} {0x01};";
                        }

                        break;

                    case 0xfc: // else
                        newlines[i] = $"{cmd} {0x4000} {0x40} {0x00}";
                        break;


                    case 0xf0: // break
                        newlines[i] = $"{cmd} {0x25} {0x4000} {0x01}";
                        break;
                    default:
                        Console.WriteLine("We are not there");
                        return;

                }

            }

            newlines[newlines.Length-1] = $"{END_OF_FILE}";
            for(int i = 0; i < newlines.Length; i++)
            {
                byte[] tmp = Encoding.ASCII.GetBytes(newlines[i]);
                byteArrays.Add(tmp);
            }
            foreach(byte[] item in byteArrays)
            {
                AppendAllBytes(outFile, item);
            }
        }
    }
}