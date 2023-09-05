using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Concatenates, hashes, and converts PII elements into hexadecimal digest.
    /// PII elements should be already normalized by the time they are provided here.
    /// </summary>
    public class LdsHasher : ILdsHasher
    {
        /// <summary>
        /// Public entrypoint for class that will perform all steps needed
        /// given normalized data elements.
        /// </summary>
        /// <param name="lname">normalized last name of individual</param>
        /// <param name="dob">normalized date of birth of individual</param>
        /// <param name="ssn">normalized social security number of individual</param>
        public string Run(string lname, string dob, string ssn)
        {
            return ToHexDigest(ToHash(Concatenate(lname, dob, ssn)));
        }

        /// <summary>
        /// Helper method to join normalized data elements together according to PPRL specifications.
        /// </summary>
        /// <param name="lname">normalized last name of individual</param>
        /// <param name="dob">normalized date of birth of individual</param>
        /// <param name="ssn">normalized social security number of individual</param>
        private string Concatenate(string lname, string dob, string ssn)
        {
            return String.Join(",", new List<string>() { lname, dob, ssn });
        }

        /// <summary>
        /// Helper method that hashes concatenated PII elements according to PPRL specifications.
        /// </summary>
        /// <param name="input">concatenated PII elements of individual</param>
        private byte[] ToHash(string input)
        {
            SHA512 sha = SHA512.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        /// <summary>
        /// Helper method that converts hashed binary array to hexidecimal digest
        /// according to PPRL specifications in order to send it.
        /// </summary>
        /// <param name="bytes">binary array of concatenated individual PII</param>
        private string ToHexDigest(byte[] bytes)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
