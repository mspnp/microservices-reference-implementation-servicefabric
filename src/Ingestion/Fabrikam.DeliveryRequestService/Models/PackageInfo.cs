using System;

namespace DeliveryRequestService.Models
{
    [Serializable()]
    public class PackageInfo
    {
        public string PackageId { get; set; }

        public ContainerSize ContainerSize { get; set; }

        public double Weight { get; set; }

        public string Tag { get; set; }

        public override string ToString()
        {
            return $"PackageInfo [packageId={PackageId}, size={Enum.GetName(typeof(ContainerSize), ContainerSize)}, weight={Weight}, tag={Tag}]";
        }
    }
}
