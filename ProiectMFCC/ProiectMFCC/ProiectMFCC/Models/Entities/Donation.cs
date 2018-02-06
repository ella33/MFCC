using System;

namespace ProiectMFCC.Models.Entities
{
    public class Donation
    {
        public Donation(int id, int projectId, int donorId, string submissionDate, int amount)
        {
            Id = id;
            ProjectId = projectId;
            DonorId = donorId;
            SubmissionDate = submissionDate;
            Amount = amount;
        }

        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int DonorId { get; set; }
        public string SubmissionDate { get; set; }
        public int Amount { get; set; }
    }
}