using ID.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ID.Api.Extensions
{
    public static class CandidateExtensions
    {
        public static bool IsValid(this BaseCandidate candidate)
        {
            bool isValid = candidate.PERSONAL_NUMBER != 0
                && candidate.ID_NUMBER != 0
                && !string.IsNullOrEmpty(candidate.FIRST_NAME)
                && !string.IsNullOrEmpty(candidate.LAST_NAME);
            // && !string.IsNullOrEmpty(candidate.EMAIL);

            if (!isValid)
            {

                string message = $"\nPERSONAL_NUMBER={candidate.PERSONAL_NUMBER}; " +
                    $"ID_NUMBER={candidate.ID_NUMBER}; " +
                    $"FIRST_NAME={Convert.ToString(candidate.FIRST_NAME)}; " +
                    $"LAST_NAME={Convert.ToString(candidate.LAST_NAME)}; " +
                    $"EMAIL={Convert.ToString(candidate.EMAIL)}";

                //LogManager.Logger.Error("Candidate not valid because one from property empty or null, such as: " + message);
            }

            return isValid;
        }

        public static List<T> LoadJson<T>() where T : class
        {
            using (StreamReader sr = new StreamReader("../test.json"))
            {
                string json = sr.ReadToEnd();
                List<T> candidates = JsonConvert.DeserializeObject<List<T>>(json);
                return candidates;
            }
        }
    }
}

