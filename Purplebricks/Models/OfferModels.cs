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
    [Table("Offer")]
    public class Offer
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? OfferID { get; set; }
        public int? PropertyID { get; set; }
        public int? UserID { get; set; }
        public double? Value { get; set; }
        public bool? Status { get; set; }
    }

    public class OfferModel
    {
        public int? OfferID { get; set; }
        public int? PropertyID { get; set; }
        public double? Value { get; set; }
        public string UserName { get; set; }

        public static IEnumerable<Offer> ByUserName(string User)
        {
            int? UserID = UserProfile.GetUserByName(User).UserId;

            using (PropertyContext context = new PropertyContext())
            {
                return context.Offers.Where(x => x.UserID == UserID).ToArray();
            }
        }

        public static IEnumerable<OfferModel> ByPropertyID(int? propertyID)
        {
            using (PropertyContext context = new PropertyContext())
            {
                return context.Database.SqlQuery<OfferModel>(@"
                    select OfferID, PropertyID, Value, UserName FROM Offer 
                    inner join UserProfile on UserProfile.UserId = Offer.UserID WHERE PropertyID = @PropertyID and Status <> 1", new SqlParameter("@PropertyID", propertyID)).ToArray();
            }
        }

        public static void Accept(int? offerID, int? propertyID)
        {
            using (PropertyContext context = new PropertyContext())
            {
                context.Database.ExecuteSqlCommand(@"update Property set SoldToOffer = @OfferID where PropertyId = @PropertyID",
                    new SqlParameter("@OfferID", offerID), new SqlParameter("@PropertyID", propertyID));
            }
        }

        public static void Reject(int? offerID)
        {
            using (PropertyContext context = new PropertyContext())
            {
                context.Database.ExecuteSqlCommand(@"update Offer set Status = 1 where OfferId = @OfferID",
                    new SqlParameter("@OfferID", offerID));
            }
        }

        public void PushToDB(string User)
        {
            int? UserID = UserProfile.GetUserByName(User).UserId;

            using (PropertyContext context = new PropertyContext())
            {

                if (OfferID == null)
                {
                    var offers = context.Offers.Where(x => (x.PropertyID == PropertyID && UserID == x.UserID)).ToArray();

                    if (offers.Any())
                    {
                        offers.First().Value = Value;
                        offers.First().Status = false;
                    }
                    else
                    {
                        context.Offers.Add(new Offer
                            {
                                UserID = UserID,
                                PropertyID = PropertyID,
                                Value = Value,
                                Status = false
                            });
                    }

                }
                else
                {
                    context.Offers.Attach(new Offer
                    {
                        OfferID = OfferID,
                        UserID = UserID,
                        PropertyID = PropertyID,
                        Value = Value,
                        Status = false
                    });
                }

                context.SaveChanges();
            }


        }
    }
}