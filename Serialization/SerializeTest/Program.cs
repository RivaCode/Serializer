using System.Diagnostics;
using System.IO;
using DotNetSerializer;

namespace SerializeTest
{
    public class FullName
    {
        public bool Equals(FullName other)
        {
            return string.Equals(_firstName, other._firstName) && string.Equals(_lastName, other._lastName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FullName && Equals((FullName)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_firstName != null ? _firstName.GetHashCode() : 0) * 397) ^ (_lastName != null ? _lastName.GetHashCode() : 0);
            }
        }

        public static bool operator ==(FullName left, FullName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FullName left, FullName right)
        {
            return !left.Equals(right);
        }

        private string _firstName;
        private string _lastName;

        public FullName()
        {

        }

        public FullName(string firstName, string lastName)
        {
            _firstName = firstName;
            _lastName = lastName;
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }
    }

    public class Address
    {
        protected bool Equals(Address other)
        {
            return string.Equals(Street, other.Street) && string.Equals(City, other.City) && string.Equals(ZipCode, other.ZipCode) && string.Equals(PhoneNumber, other.PhoneNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Address)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Street != null ? Street.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (City != null ? City.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ZipCode != null ? ZipCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PhoneNumber != null ? PhoneNumber.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Address left, Address right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !Equals(left, right);
        }

        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class Person
    {
        protected bool Equals(Person other)
        {
            return Name.Equals(other.Name) && Age == other.Age && Equals(Address, other.Address);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Person)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Age;
                hashCode = (hashCode * 397) ^ (Address != null ? Address.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Person left, Person right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Person left, Person right)
        {
            return !Equals(left, right);
        }

        public FullName Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public Person Spouse { get; set; }
    }


    public class Serializer
    {
        public static void Serialize<T>(T o, Stream stream)
        {
            ObjectSerializer ser = new ObjectSerializer();
            ser.Serialize(o, stream);
        }

        public static T Deserialize<T>(Stream stream)
        {
            ObjectSerializer ser = new ObjectSerializer();
            return (T)ser.Deserialize(stream);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var alon = new Person
            {
                Name = new FullName("Alon", "Fliess"),
                Age = 43,
                Address = new Address
                {
                    Street = "Azmaut 10",
                    City = "Binyamina",
                    ZipCode = "30500",
                    PhoneNumber = "+972-546160636"
                }
            };

            var liat = new Person
            {
                Name = new FullName("Liat", "Fliess"),
                Age = 40,
                Address = alon.Address,
                Spouse = alon
            };
            alon.Spouse = liat;

            using (var buffer = new MemoryStream())
            {
                Serializer.Serialize(liat, buffer);
                buffer.Seek(0, SeekOrigin.Begin);
                var newLiat = Serializer.Deserialize<Person>(buffer);

                Trace.Assert(liat == newLiat, "Fail, object are not logicaly the same");
                Trace.Assert(newLiat != null && newLiat.Spouse != null && newLiat.Spouse.Address != null && newLiat.Address != null && ReferenceEquals(newLiat.Address, newLiat.Spouse.Address), "Address is not the same object");
                Trace.Assert(newLiat != null && newLiat.Spouse != null && newLiat.Spouse.Spouse != null && ReferenceEquals(liat.Spouse.Spouse, liat), "The new Liat and new Alon don't refer each other");
            }
        }
    }
}

