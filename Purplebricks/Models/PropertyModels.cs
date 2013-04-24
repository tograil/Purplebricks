using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Purplebricks.Models
{
    public class PropertyContext : DbContext
    {
        public PropertyContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<UserProfile> Users { get; set; }


    }

    [Table("Property")]
    public class Property
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? PropertyId { get; set; }
        public int UserID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int? SoldToOffer { get; set; }

        public static IEnumerable<Property> OnSale()
        {
            using (PropertyContext context = new PropertyContext())
            {
                return context.Database.SqlQuery<Property>(
                    @"select Property.* from Property
                        inner join UserProfile on UserProfile.UserID = Property.UserID
                        where SoldToOffer IS null").ToArray();
            }
        }

        public static IEnumerable<Property> OnSale(string User)
        {
            using (PropertyContext context = new PropertyContext())
            {
                return context.Database.SqlQuery<Property>(
                    @"select Property.* from Property
                        inner join UserProfile on UserProfile.UserID = Property.UserID
                        where UserName = @UserName AND SoldToOffer IS null", new SqlParameter("@UserName", User)).ToArray();
            }
        }

        public static IEnumerable<SellerPropertyModel> OnSaleWithOffers(string User)
        {
            using (PropertyContext context = new PropertyContext())
            {
                try
                {
                    return context.Database.SqlQuery<SellerPropertyModel>(
                        @"select Property.PropertyId as PropertyId,
                        Property.Name as Name,
                        Property.Type as Type,
                        Property.Description as Description,
                        (select COUNT(OfferID) from Offer where Offer.PropertyID = Property.PropertyID AND (Offer.Status = 0 OR Offer.Status IS Null)) as OffersCount
                        from Property
                        inner join UserProfile on UserProfile.UserID = Property.UserID
                        where UserName = @UserName AND SoldToOffer IS null
                        ", new SqlParameter("@UserName", User)).ToArray();
                }
                catch
                {
                    return new List<SellerPropertyModel>();
                }
            }
        }

    }

    public class SellerPropertyModel
    {
        public int? PropertyId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int OffersCount { get; set; }
    }

    public class PropertyModel
    {
        public int? PropertyID { get; set; }
        
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public static PropertyModel ByID(int? PropertyID)
        {
            using (PropertyContext context = new PropertyContext())
            {
                return context.Properties.Where(x => x.PropertyId == PropertyID).Select(x => new PropertyModel { 
                    PropertyID = x.PropertyId,
                    Name = x.Name,
                    Description = x.Description,
                    Type = x.Type

                } ).FirstOrDefault();
            }
        }

        public static void ImportToDB(string User, IEnumerable<PropertyModel> properties)
        {
            int? UserID = UserProfile.GetUserByName(User).UserId;

            using (PropertyContext context = new PropertyContext())
            {
                foreach (var property in properties)
                {
                    context.Properties.Add(new Property
                    {
                        Name = property.Name,
                        UserID = UserID.HasValue ? UserID.Value : 0,
                        Type = property.Type,
                        Description = property.Description,
                    });
                }

                context.SaveChanges();
            }
        }

        public void PushToDB(string User)
        {
            int? UserID = UserProfile.GetUserByName(User).UserId;

            using (PropertyContext context = new PropertyContext())
            {
                if (PropertyID == null)
                {
                    context.Properties.Add(new Property
                    {
                        Name = Name,
                        UserID = UserID.HasValue ? UserID.Value : 0,
                        Type = Type,
                        Description = Description,
                    });
                }
                else
                {
                    context.Properties.Attach(new Property
                    {
                        PropertyId = PropertyID,
                        UserID = UserID.HasValue ? UserID.Value : 0,
                        Name = Name,
                        Type = Type,
                        Description = Description,
                    });
                }

                context.SaveChanges();
            }
        }
    }
}