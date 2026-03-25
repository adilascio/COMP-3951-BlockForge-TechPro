using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// BlockForge PayloadTransformer 
/// Author: Angus Grewal
/// Date: Mar 25 2026
/// Source: Self-written, with AI coaching. All code submitted is human written, based on ChatGPT guidance.
/// </summary>
namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Simple text transformation layer that uses a Caesar Cipher to illustrate where actual secure encryption should go into our file format pipeline.
    /// In actual production, this should use a much more secure algorithm to ensure our file type can be more secure from unauthorized tampering.
    /// eg. opening the file in a plaintext editor to deliberately bypass our software-based guardrails.
    /// </summary>
    public class PayloadTransformer
    {
        /// <summary>
        /// Used to track how much to shift the 'encrypted' payload's contents by.
        /// </summary>
        private int Shift { get; }

        /// <summary>
        /// Helper method that will shift the actual contents of the message, a-z or A-Z.
        /// </summary>
        /// <param name="c">the character before shifting.</param>
        /// <param name="offset">the number of characters to shift by.</param>
        /// <returns>the shifted character.</returns>
        private char CaesarCipher(char c, int offset)
        {
            char insert = c;
            if (c >= 'a' && c <= 'z')
            {
                int difference = c - 'a';
                int shifted = (difference + offset) % 26;
                if (shifted < 0)
                {
                    shifted += 26;
                }
                insert = (char)('a' + shifted);
            }
            else if (c >= 'A' && c <= 'Z')
            {
                int difference = c - 'A';
                int shifted = (difference + offset) % 26;
                if (shifted < 0)
                {
                    shifted += 26;
                }
                insert = (char)('A' + shifted);
            }

            return insert;
        }

        /// <summary>
        /// Constructor for a PayloadTransformer. Instantiate with a shift value.
        /// </summary>
        /// <param name="shift">the shift value.</param>
        public PayloadTransformer(int shift)
        {
            this.Shift = shift % 26;
            if (this.Shift < 0)
            {
                this.Shift += 26;
            }
        }

        /// <summary>
        /// Function to encrypt characters according to Caesar Cipher rules.
        /// </summary>
        /// <param name="input">the input string.</param>
        /// <returns>the ciphered string.</returns>
        public string Scramble(string input)
        {
            StringBuilder output = new StringBuilder();

            foreach (char c in input)
            {
               output.Append(CaesarCipher(c, Shift));
            }

            return output.ToString();
        }

        /// <summary>
        /// Function to reverse encryption according to Caesar Cipher rules.
        /// </summary>
        /// <param name="input">the input string, ciphered.</param>
        /// <returns>the original string.</returns>
        public string Unscramble(string input)
        {
            StringBuilder output = new StringBuilder();

            foreach (char c in input)
            {
                output.Append(CaesarCipher(c, -Shift));
            }

            return output.ToString();
        }
    }
}
