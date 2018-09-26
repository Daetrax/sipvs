using spracovanieInfo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spracovanieInfo
{
    public class Request
    {
        public string Name;
        public string Surname;
        public string Street;
        public int StreetNumber;
        public string Country;
        public string City;
        public string Zip;
        public string RequestDate;
        public int LoanPeriod;

        public Book[] BookList;


        public Request(string name, string surname, 
                           string street, int streetNumber, 
                           string country, string city, string zipCode,
                           int loanPeriod, Book[] booksList)
        {
            this.Name = name;
            this.Surname = surname;
            this.Street = street;
            this.StreetNumber = streetNumber;
            this.Country = country;
            this.City = city;
            this.Zip = zipCode;
            this.LoanPeriod = loanPeriod;
            this.BookList = booksList;

            this.RequestDate = DateTime.Now.ToString("yyyy-MM-dd");
            
        }

        public Request()
        {

        }
    }
}
