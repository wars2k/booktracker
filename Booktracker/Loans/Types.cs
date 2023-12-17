namespace bookTrackerApi.Loans {

    public static class Types {

        ///<summary>This is the class that will be returned to the client when it requests a list
        ///of all loans. Should be able to filter this by book, status, and loanee</summary>
        public class BasicLoanInfo {

            public int? Id { get; set; }
            public string? Status { get; set; }
            public string? LoanDate { get; set; }
            public string? ReturnDate { get; set; }
            public int? BookListID { get; set; }
            public string? BookTitle { get; set; }
            public int? LoaneeID { get; set; }
            public string? LoaneeName { get; set; }

        }

        ///<summary>Contains all info that might be collected when documenting a new loan.</summary>
        public class NewLoan {

            public int? BookListID { get; set; }
            public int? LoaneeID { get; set; }
            public string? Date { get; set; }
            public string? ReturnDate { get; set; }
            public string? Comment { get; set; }

        }

        ///<summary>Contains all info that might be collected when creating a new 'loanee'.</summary>
        public class NewLoanee {

            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Note { get; set; }

        }

        ///<summary>Contains all info that the client needs for each loanee.</summary>
        public class LoaneeInfo {

            public int? Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Note { get; set; }

        }

        ///<summary>This class will be used to populate the drop-downs on the client side for loanees.</summary>
        public class LoaneeName {
            public int? Id { get; set; }
            public string? Name { get; set; }
        }

    }

}