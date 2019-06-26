using System;

namespace DeliveryRequestService.Models
{
    [Serializable()]
    public class InternalDeliveryRequest
    {

        public string DeliveryId { get; set; }

        public string OwnerId { get; set; }

        public string PickupLocation { get; set; }

        public string DropoffLocation { get; set; }

        public DateTimeOffset PickupTime { get; set; }

        public string Deadline { get; set; }

        public bool Expedited { get; set; }

        public ConfirmationRequired ConfirmationRequired { get; set; }

        public PackageInfo PackageInfo { get; set; }

        public InternalDeliveryRequest(string id, string ownerId, string pickupLocation, string dropoffLocation, DateTimeOffset pickupTime, string deadline, bool expedited, ConfirmationRequired confirmationRequired, PackageInfo packageInfo)
        {
            DeliveryId = id;
            OwnerId = ownerId;
            PickupLocation = pickupLocation;
            DropoffLocation = dropoffLocation;
            PickupTime = pickupTime;
            Deadline = deadline;
            Expedited = expedited;
            ConfirmationRequired = confirmationRequired;
            PackageInfo = packageInfo;
        }

        public InternalDeliveryRequest()
        {
        }

        public override string ToString() => $"InternalDelivery [deliveryId={DeliveryId}, ownerId={OwnerId}, pickupLocation={PickupLocation}, dropoffLocation={DropoffLocation}, pickupTime={PickupTime}, deadline={Deadline}, expedited={Expedited}, confirmationRequired={Enum.GetName(typeof(ConfirmationRequired), ConfirmationRequired)}, packageInfo={PackageInfo}]";
    }
}