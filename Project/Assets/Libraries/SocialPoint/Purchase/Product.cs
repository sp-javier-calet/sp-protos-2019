
namespace SocialPoint.Purchase
{
    public struct Product
    {
        public string Id { get; private set; }

        public string Locale { get; private set; }

        public float Price { get; private set; }

        public string Currency { get; private set; }

        public Product(string id, string locale, float price, string currency) : this()
        {
            Id = id;
            Locale = locale;
            Price = price;
            Currency = currency;
        }

        public override string ToString()
        {
            return string.Format("[Product details: Id = {0}, Locale = {1}, Price = {2}, Currency = {3}]",
                Id, Locale, Price, Currency);
        }
    }
}