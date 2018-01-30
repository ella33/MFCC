using System;

namespace ProiectMFCC.Models.Entities
{
    public class Donation
    {
        public Donation(int projectId, int donorId, DateTime submissionDate, int amount)
        {
            ProjectId = projectId;
            DonorId = donorId;
            SubmissionDate = submissionDate;
            Amount = amount;
        }

        public int ProjectId { get; set; }
        public int DonorId { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int Amount { get; set; }
    }
}