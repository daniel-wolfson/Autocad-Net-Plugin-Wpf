using System;

namespace ID.WebApi.Model
{
    public class User : LoginModel
    {
        //public string Email { get; set; } //character varying(256) COLLATE pg_catalog."default" NOT NULL,
        public bool EmailConfirmed { get; set; } //bit(1) NOT NULL,
        //public string PasswordHash { get; set; } 
        //character varying(256) COLLATE pg_catalog."default",
        public string SecurityStamp { get; set; } //character varying(256) COLLATE pg_catalog."default",
        public string PhoneNumber { get; set; } //character varying(256) COLLATE pg_catalog."default",
        public bool PhoneNumberConfirmed { get; set; } //bit(1) NOT NULL,
        public DateTime LockoutEndDateUtc { get; set; } //bit(1) NOT NULL,
        public bool TwoFactorEnabled { get; set; } //bit(1) NOT NULL,
        public bool LockoutEnabled { get; set; } //bit(1) NOT NULL,
        public int AccessFailedCount { get; set; } //integer NOT NULL,
        //public string UserName { get; set; } //character varying(256) COLLATE pg_catalog."default" NOT NULL,
        public int Id { get; set; } //character varying(256) COLLATE pg_catalog."default" NOT NULL,
    }
}

