using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ID.Api.Models
{
    public class PsygateCandidate : BaseCandidate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Candidate id")]
        public int CANDIDATE_ID { get; set; }

        [DisplayName("Temp password")]
        public string TEMPPASS { get; set; }

    }

    public class BaseCandidate
    {
        [DisplayName("Personal number")]
        public double PERSONAL_NUMBER { get; set; } // TODO: ?= CandidateId

        [DisplayName("id number (tz)")]
        public double ID_NUMBER { get; set; }   // TODO: ?= IdValue

        [DisplayName("First name")]
        public string FIRST_NAME { get; set; }   // TODO: ?= FirstName

        [DisplayName("Last name")]
        public string LAST_NAME { get; set; }   // TODO: ?= LastName

        // public string Mobile { get; set; }
        // public string MobileCode { get; set; }

        private string _email = "";
        public string EMAIL
        {
            get { return _email; }
            set { _email = value ?? ""; }
        }

        [DisplayName("Mobile with code")]
        public string PHONE_NUMBER2 { get; set; }
    }
}

