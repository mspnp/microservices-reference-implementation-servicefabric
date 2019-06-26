using System;

namespace DeliveryRequestService.Models
{
    public class DeliveryRequest
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

        public override string ToString() => $"DeliveryRequest [deliveryId={DeliveryId}, ownerId={OwnerId}, pickupLocation={PickupLocation}, dropoffLocation={DropoffLocation}, pickupTime={PickupTime}, deadline={Deadline}, expedited={Expedited}, confirmationRequired={Enum.GetName(typeof(ConfirmationRequired), ConfirmationRequired)}, packageInfo={PackageInfo}]";

    }
}
