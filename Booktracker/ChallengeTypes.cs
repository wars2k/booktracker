namespace bookTrackerApi {
    
    public static class ChallengeTypes {

        public class Challenge {
            
            public int? Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Date_created { get; set ;}
            public string? Status { get; set; }
            public string? Type { get; set; }
            public string? SubType { get; set; }
            public string? Start_date { get; set ;}
            public string? End_date { get; set ;}
            public int? Goal { get; set; }
            public int? Count { get; set; }
            public string? Record { get; set; }
        }

        public class LocalChallenge {
            
            public int? Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Date_created { get; set ;}
            public string? Status { get; set; }
            public string? Type { get; set; }
            public string? SubType { get; set; }
            public DateTime? Start_date { get; set ;}
            public DateTime? End_date { get; set ;}
            public int? Goal { get; set; }
            public int? Count { get; set; }
            public string? Record { get; set; }
        }

        public class NewChallenge {

            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Type { get; set; }
            public string? SubType { get; set; }
            public string? Start_date { get; set; }
            public string? End_date { get; set; }
            public string? Goal { get; set; }
            
        }

    }

}