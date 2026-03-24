using System.Numerics;
using System.Text;

namespace ProjectSecurity.Services
{
    public class SecurityService
    {
        // ===== Utility functions =====
        public static string TextToBinary(string text, int length)
        {
            // ensure the text has at least the specified number of characters
            if (text.Length < length)
            {
                text = text.PadRight(length, ' ');
            }
            else if (text.Length > length)
            {
                text = text.Substring(0, length);
            }
            string binaryString = "";
            foreach (char c in text)
            {
                binaryString += Convert.ToString(c, 2).PadLeft(8, '0');
            }
            return binaryString; // return the binary string
        }

        public static string BinaryToText(string binary)
        {
            string text = "";
            for (int i = 0; i < binary.Length; i += 8)
            {
                string byteString = binary.Substring(i, 8);
                char c = (char)Convert.ToInt32(byteString, 2);
                text += c;
            }
            return text.TrimEnd(); // delete the extra spaces at the end of the text if there are any
        }

        // New hex utility functions
        public static string HexToBinary(string hex)
        {
            if (hex.Length != 16)
            {
                throw new ArgumentException("Hex string must be exactly 16 characters (64 bits)");
            }
            string binary = "";
            foreach (char c in hex)
            {
                int value = Convert.ToInt32(c.ToString(), 16);
                binary += Convert.ToString(value, 2).PadLeft(4, '0');
            }
            return binary; // 64-bit binary string
        }

        public static string BinaryToHex(string binary)
        {
            if (binary.Length != 64)
            {
                throw new ArgumentException("Binary string must be exactly 64 bits");
            }
            string hex = "";
            for (int i = 0; i < 64; i += 4)
            {
                string nibble = binary.Substring(i, 4);
                int value = Convert.ToInt32(nibble, 2);
                hex += value.ToString("X1");
            }
            return hex; // 16-character hex string
        }

        // ===== Encrypt =====
        // ******* Ceasar *******
        public string CaesarEncrypt(string text, int key)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                result.Append((char)(c + key));
            }

            return result.ToString();
        }

        // ******* Playfair *******
        public string PlayfairEncrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            // create key matrix
            char[,] keyMatrix = CreateKeyMatrix(key);
            // split text into list of pair
            List<string> pairs = PrepareText(text, out Dictionary<int, char> special);

            foreach (var pair in pairs)
            {
                char a = pair[0];
                char b = pair[1];

                FindPosition(keyMatrix, a, out int r1, out int c1);
                FindPosition(keyMatrix, b, out int r2, out int c2);

                // if 2 char have the same row in the matrix then move the col forward
                if (r1 == r2)
                {
                    result.Append(keyMatrix[r1, (c1 + 1) % 5]);
                    result.Append(keyMatrix[r2, (c2 + 1) % 5]);
                }
                // if 2 char have the same col in the matrix then move the row forward
                else if (c1 == c2)
                {
                    result.Append(keyMatrix[(r1 + 1) % 5, c1]);
                    result.Append(keyMatrix[(r2 + 1) % 5, c2]);
                }
                // if it is random char, create a rectangle and first char is another top corner, second char is another bottom corner
                else
                {
                    result.Append(keyMatrix[r1, c2]);
                    result.Append(keyMatrix[r2, c1]);
                }
            }
            foreach (var item in special)
            {
                result.Insert(item.Key, item.Value);
            }

            return result.ToString();
        }

        public char[,] CreateKeyMatrix(string key)
        {
            // chang to uppercase and replace J to I
            key = key.ToUpper().Replace("J", "I");
            
            string alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";

            // use HashSet to ignore duplication char
            HashSet<char> used = new HashSet<char>();
            List<char> result = new List<char>();

            // remove dup char from key and add to result
            foreach (char c in key)
            {
                if (char.IsLetter(c) && !used.Contains(c))
                {
                    used.Add(c);
                    result.Add(c);
                }
            }

            // add remain alphabet char and remove dup char
            foreach (char c in alphabet)
            {
                if (!used.Contains(c))
                {
                    used.Add(c);
                    result.Add(c);
                }
            }

            // create 5x5 martrix
            char[,] matrix = new char[5, 5];
            for (int i = 0; i < 25; i++)
            {
                matrix[i / 5, i % 5] = result[i];
            }

            return matrix;
        }

        public void FindPosition(char[,] matrix, char c, out int row, out int col)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (matrix[i, j] == c)
                    {
                        row = i;
                        col = j;
                        return;
                    }
                }
            }

            row = -1;
            col = -1;
        }

        public List<string> PrepareText(string text, out Dictionary<int, char> special)
        {

            special = new Dictionary<int, char>();

            for (int i = 0; i < text.Length; i++)
            {
                // store special char into dict
                if (!char.IsLetter(text[i]))
                {
                    special.Add(i, text[i]);
                }
            }

            // list of pairs split from text
            List<string> pairs = new List<string>();
            // change text to uppercase and replace J to I
            text = text.ToUpper().Replace("J", "I");
            // eliminate all special char
            text = new string(text.Where(char.IsLetter).ToArray());

            for (int i = 0; i < text.Length; i+=2)
            {                
                // add X if the char is at the end of text
                if (i + 1 >= text.Length)
                {
                    pairs.Add(text[i] + "X");
                }
                // add X between dup char
                else if (text[i] == text[i+1])
                {
                    pairs.Add(text[i] + "X");
                    i--; // back to the second dup char
                }
                // add pair of char and the next char
                else
                {
                    pairs.Add(text[i].ToString() + text[i+1]);
                }
            }
            return pairs;
        }

        // ******* Vigenere repeat key *******
        public string VigenereRepeatEncrypt(string text, string key)
        {
            text = text.ToUpper();
            key = key.ToUpper();

            StringBuilder result = new StringBuilder();

            int keyIndex = 0;

            foreach (char c in text)
            {
                // skip the step if it is not a letter
                if (!char.IsLetter(c))
                {
                    result.Append(c);
                    continue;
                }

                // create shift value that repeat from key to move the char of text
                int shift = key[keyIndex % key.Length] - 'A';

                // first subtract the char with A = 26 to have its position then add with shift value to move its position. Finally mod it with 26 to roll back to the start of alphabet if the char after move's value is out of alphabet and add it with A = 26 to have result
                char encrypted = (char)(((c - 'A' + shift) % 26) + 'A');

                result.Append(encrypted);

                // move to another char of key
                keyIndex++;
            }
            return result.ToString();
        }

        public string VigenereAutoEncrypt(string text, string key)
        {
            text = text.ToUpper();
            key = key.ToUpper();

            StringBuilder result = new StringBuilder();
            // generate key based on text
            StringBuilder fullkey = new StringBuilder(key);
            // add untill the key has the length equal to the text
            for (int i = 0; fullkey.Length < text.Length && i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    fullkey.Append(text[i]);
                }
            }

            int keyIndex = 0;

            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                {
                    result.Append(c);
                    continue;
                }

                int shift = fullkey[keyIndex] - 'A';
                char encrypted = (char)(((c - 'A') + shift) % 26 + 'A');

                result.Append(encrypted);

                keyIndex++;
            }
            return result.ToString();
        }

        // ===== Decrypt =====
        // ******* Ceasar *******
        public string CaesarDecrypt(string text, int key)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                result.Append((char)(c - key));
            }

            return result.ToString();
        }

        // ******* Playfair *******
        public string PlayfairDecrypt(string text, string key)
        {
            char[,] matrix = CreateKeyMatrix(key);

            Dictionary<int, char> special = new Dictionary<int, char>();
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsLetter(text[i]))
                {
                    special.Add(i, text[i]);
                }
            }
            text = new string(text.Where(char.IsLetter).ToArray());
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < text.Length; i += 2)
            {
                char a = text[i];
                char b = text[i + 1];

                FindPosition(matrix, a, out int r1, out int c1);
                FindPosition(matrix, b, out int r2, out int c2);

                if (r1 == r2)
                {
                    result.Append(matrix[r1, (c1 + 4) % 5]);
                    result.Append(matrix[r2, (c2 + 4) % 5]);
                }
                else if (c1 == c2)
                {
                    result.Append(matrix[(r1 + 4) % 5, c1]);
                    result.Append(matrix[(r2 + 4) % 5, c2]);
                }
                else
                {
                    result.Append(matrix[r1, c2]);
                    result.Append(matrix[r2, c1]);
                }
            }

            foreach (var item in special)
            {
                result.Insert(item.Key, item.Value);
            }

            return result.ToString();
        }
        // ******* Vigenere *******
        public string VigenereRepeatDecrypt(string text, string key)
        {
            text = text.ToUpper();
            key = key.ToUpper();
            
            StringBuilder result = new StringBuilder();

            int keyIndex = 0;

            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                {
                    result.Append(c);
                    continue;
                }

                int shift = key[keyIndex % key.Length] - 'A';
                char decrypted = (char)(((c - 'A' - shift + 26) % 26) + 'A');
                result.Append(decrypted);
                keyIndex++;
            }
            return result.ToString();
        }

        public string VigenereAutoDecrypt(string text, string key)
        {
            text = text.ToUpper();
            key = key.ToUpper();

            StringBuilder result = new StringBuilder();
            StringBuilder fullkey = new StringBuilder(key);
            int keyIndex = 0;

            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                {
                    result.Append(c);
                    continue;
                }

                int shift = fullkey[keyIndex] - 'A';
                char decrypted = (char)(((c - 'A') + 26 - shift) % 26 + 'A');
                
                result.Append(decrypted);
                fullkey.Append(decrypted);

                keyIndex++;
            }

            return result.ToString();
        }

        // ===== Euler =====
        // Apply Euler Formula φ(n) = n(1 − 1/p1)(1 − 1/p2)...
        public BigInteger Euler(BigInteger n)
        {
            // declare result = n then subtract by the time follow the formula
            BigInteger result = n;
            // go from the smallest prime number to < sqrt(n)
            // n = a x b; only one of two number can be smaller than sqrt(n) and the other is bigger because a x b != n if a, b > sqrt(n)
            for (BigInteger p = 2; p * p <= n; p++)
            {
                // find the small prime number to subtract
                if (n % p == 0)
                {
                    // divide it till it out of that prime number. eg: 14 -> /2 x1 -> 7
                    while (n % p == 0)
                    {
                        n /= p;
                    }
                    
                    // this is the formula n(1 - 1/p)
                    result -= result / p;
                }
            }

            // check the situation when n contains the prime number bigger than sqrt(n)
            if (n > 1)
            {
                result -= result / n;
            }

            return result;
        }

        // ===== DES =====
        // ******* Encryption *******
        // ------- IP function -------
        // Initial Permutation matrix
        static int[] IP =
        {
            58,50,42,34,26,18,10,2,
            60,52,44,36,28,20,12,4,
            62,54,46,38,30,22,14,6,
            64,56,48,40,32,24,16,8,
            57,49,41,33,25,17,9,1,
            59,51,43,35,27,19,11,3,
            61,53,45,37,29,21,13,5,
            63,55,47,39,31,23,15,7  
        };
        // input 64 bit array
        public static string IPermutaion(string x)
        {
            char[] y = new char[64];
            for (int i = 0; i < 64; i++)
            {
                // return new permutation index. minus 1 because the array index start from 0
                y[i] = x[IP[i] - 1];
            }

            // transfer char array to string array
            return new string(y);
        }

        // ------- Split function -------
        // split the string to 32 bit left and right
        public static void SPLIT(string x, out string L, out string R)
        {
            L = x.Substring(0, 32);
            R = x.Substring(32, 32);
        }

        // ------- Expansion function -------
        // Expand matrix
        static int[] E =
        {
            32,1,2,3,4,5,
            4,5,6,7,8,9,
            8,9,10,11,12,13,
            12,13,14,15,16,17,
            16,17,18,19,20,21,
            20,21,22,23,24,25,
            24,25,26,27,28,29,
            28,29,30,31,32,1
        };
        // expand from 32 bit string to 48 bit string
        public static string Expand(string R)
        {
            char[] R1 = new char[48];

            for (int i = 0; i < 48; i++)
            {
                R1[i] = R[E[i] - 1];
            }

            return new string(R1);
        }

        // ------- XOR function -------
        // if the 2 bits are the same, the value is 0; otherwise, it is 1
        public static string XOR(string a, string b)
        {
            char[] res = new char[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                res[i] = (a[i] == b[i]) ? '0' : '1';
            }
            return new string(res);
        }

        // ------- S-BOX -------
        // 8 box
        static int[,,] SBOX =
        {
        {
        {14,4,13,1,2,15,11,8,3,10,6,12,5,9,0,7},
        {0,15,7,4,14,2,13,1,10,6,12,11,9,5,3,8},
        {4,1,14,8,13,6,2,11,15,12,9,7,3,10,5,0},
        {15,12,8,2,4,9,1,7,5,11,3,14,10,0,6,13}
        },

        {
        {15,1,8,14,6,11,3,4,9,7,2,13,12,0,5,10},
        {3,13,4,7,15,2,8,14,12,0,1,10,6,9,11,5},
        {0,14,7,11,10,4,13,1,5,8,12,6,9,3,2,15},
        {13,8,10,1,3,15,4,2,11,6,7,12,0,5,14,9}
        },

        {
        {10,0,9,14,6,3,15,5,1,13,12,7,11,4,2,8},
        {13,7,0,9,3,4,6,10,2,8,5,14,12,11,15,1},
        {13,6,4,9,8,15,3,0,11,1,2,12,5,10,14,7},
        {1,10,13,0,6,9,8,7,4,15,14,3,11,5,2,12}
        },

        {
        {7,13,14,3,0,6,9,10,1,2,8,5,11,12,4,15},
        {13,8,11,5,6,15,0,3,4,7,2,12,1,10,14,9},
        {10,6,9,0,12,11,7,13,15,1,3,14,5,2,8,4},
        {3,15,0,6,10,1,13,8,9,4,5,11,12,7,2,14}
        },

        {
        {2,12,4,1,7,10,11,6,8,5,3,15,13,0,14,9},
        {14,11,2,12,4,7,13,1,5,0,15,10,3,9,8,6},
        {4,2,1,11,10,13,7,8,15,9,12,5,6,3,0,14},
        {11,8,12,7,1,14,2,13,6,15,0,9,10,4,5,3}
        },

        {
        {12,1,10,15,9,2,6,8,0,13,3,4,14,7,5,11},
        {10,15,4,2,7,12,9,5,6,1,13,14,0,11,3,8},
        {9,14,15,5,2,8,12,3,7,0,4,10,1,13,11,6},
        {4,3,2,12,9,5,15,10,11,14,1,7,6,0,8,13}
        },

        {
        {4,11,2,14,15,0,8,13,3,12,9,7,5,10,6,1},
        {13,0,11,7,4,9,1,10,14,3,5,12,2,15,8,6},
        {1,4,11,13,12,3,7,14,10,15,6,8,0,5,9,2},
        {6,11,13,8,1,4,10,7,9,5,0,15,14,2,3,12}
        },

        {
        {13,2,8,4,6,15,11,1,10,9,3,14,5,0,12,7},
        {1,15,13,8,10,3,7,4,12,5,6,11,0,14,9,2},
        {7,11,4,1,9,12,14,2,0,6,10,13,15,3,5,8},
        {2,1,14,7,4,10,8,13,15,12,9,0,3,5,6,11}
        }
        };
        // from 48 bit string to 32 bit string
        public static string SUB(string x)
        {
            string result = "";
            // loop 8 times for 8 SBOX
            for (int i = 0; i < 8; i++)
            {
                // split the string into 8 block of 6 bit length
                string block = x.Substring(i * 6, 6);
                // bit 0 and bit 5 is row, from bit 1 to bit 4 is col
                // eg: 110011 -> row = 11 = 3; col = 1001 = 9
                // the Convert.ToInt32 function convert the binary bit into 32 bit integer
                int row = Convert.ToInt32("" + block[0] + block[5], 2);
                int col = Convert.ToInt32(block.Substring(1, 4), 2);
                // find the value in the suitable SBOX
                int value = SBOX[i, row, col];
                // convert the integer back to binary bit
                // add 0 in front of the bit if it is less than 4 bit to make sure the length of bit is 4
                result += Convert.ToString(value, 2).PadLeft(4, '0');
            }

            return result;
        }
        // ------- PermutationP function -------
        // permutation P matrix
        static int[] P =
        {
            16,7,20,21,
            29,12,28,17,
            1,15,23,26,
            5,18,31,10,
            2,8,24,14,
            32,27,3,9,
            19,13,30,6,
            22,11,4,25
        };
        // permuation by the P matrix
        public static string PermutationP(string x)
        {
            string result = "";

            for (int i = 0; i < 32; i++)
            {
                result += x[P[i] - 1];
            }

            return result;
        }

        // ******* Generate Key *******
        // ------- Permutation with PC1 -------
        // PC1 matrix
        static int[] PC1 =
        {
            57,49,41,33,25,17,9,
            1,58,50,42,34,26,18,
            10,2,59,51,43,35,27,
            19,11,3,60,52,44,36,
            63,55,47,39,31,23,15,
            7,62,54,46,38,30,22,
            14,6,61,53,45,37,29,
            21,13,5,28,20,12,4
        };
        // convert 64 bit string into 56 bit string
        public static string PC1Permutation(string K)
        {
            char[] K1 = new char[56];

            for (int i = 0; i < 56; i++)
            {
                K1[i] = K[PC1[i] - 1];
            }

            return new string(K1);
        }

        // ------- Split key function -------
        // split 56 bit string to 2 bit string with 28 bit each
        public static void SplitKey(string K1, out string C, out string D)
        {
            C = K1.Substring(0, 28);
            D = K1.Substring(28, 28);
        }

        // ------- Shift left function -------
        // shift the bit from the x string to the left by s bit
        public static string ShiftLeft(string x, int s)
        {
            return x.Substring(s) + x.Substring(0, s);
        }

        // ------- PC2 permutation -------
        // PC2 matrix
        static int[] PC2 =
        {
            14,17,11,24,1,5,
            3,28,15,6,21,10,
            23,19,12,4,26,8,
            16,7,27,20,13,2,
            41,52,31,37,47,55,
            30,40,51,45,33,48,
            44,49,39,56,34,53,
            46,42,50,36,29,32
        };

        public static string PC2Permutation(string C, string D)
        {
            string CD = C + D; //56 bit
            string result = "";

            foreach (int i in PC2)
            {
                result += CD[i - 1];
            }

            return result; // 48 bit
        }

        // ******* DES encryption *******
        // F function for the right part of string
        public static string F(string R, string K)
        {
            // expand the 32 bit string into 48 bit string
            string R1 = Expand(R);
            // XOR between R1 and K
            string XR1K = XOR(R1, K);
            // S-box Permutation the 48 bit string into 32 bit string
            string SR1K = SUB(XR1K);
            // Permutation P
            string F = PermutationP(SR1K);

            return F;
        } 

        // 1 Round in DES
        public static void Round(ref string L, ref string R, string K)
        {
            // save R for later
            string temp = R;
            // F function
            string f = F(R, K);
            // Xor between R and L
            R = XOR(L, f);
            // L is R
            L = temp;
        }

        // generate 16 key for full DES round
        public static string[] GenerateKeys(string K)
        {
            string[] keys = new string[16];
            // PC1
            string K1 = PC1Permutation(K);
            // split 56 bit key into 2 part each has 28 bit
            string C, D;
            SplitKey(K1, out C, out D);
            // Schedule of Left shifts
            int[] shift = {1,1,2,2,2,2,2,2,1,2,2,2,2,2,2,1};

            for (int i = 0; i < 16; i++)
            {
                // left shift
                C = ShiftLeft(C, shift[i]);
                D = ShiftLeft(D, shift[i]);

                // PC2
                keys[i] = PC2Permutation(C, D);
            }

            return keys;
        }

        // Inverse Initial Permutation
        static int[] InverseIP =
        {
            40,8,48,16,56,24,64,32,
            39,7,47,15,55,23,63,31,
            38,6,46,14,54,22,62,30,
            37,5,45,13,53,21,61,29,
            36,4,44,12,52,20,60,28,
            35,3,43,11,51,19,59,27,
            34,2,42,10,50,18,58,26,
            33,1,41,9,49,17,57,25  
        };

        public static string IPermutaion_1(string x)
        {
            char[] y = new char[64];

            for (int i = 0; i < 64; i++)
            {
                y[i] = x[InverseIP[i] - 1];
            }

            return new string(y);
        }

        // full DES
        public static string DES(string x, string k)
        {
            // 1. IP; 64 bit
            string y = IPermutaion(x);
            // 2. Split string; 2 32 bit
            string L;
            string R;

            SPLIT(y, out L, out R);

            // 3. Generate keys; 56 bit -> 48 bit key
            string[] keys = GenerateKeys(k);

            // 4. Start 16 rounds
            for (int i = 0; i < 16; i++)
            {
                Round(ref L, ref R, keys[i]);
            }

            // 5. 32 bit swap
            string RL = R + L;

            // 6. Inverse IP
            return IPermutaion_1(RL);
        }
    }
}