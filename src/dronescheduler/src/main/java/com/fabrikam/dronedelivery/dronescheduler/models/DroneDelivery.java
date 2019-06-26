// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

package com.fabrikam.dronedelivery.dronescheduler.models;

import java.util.List;

public class DroneDelivery {
	private String deliveryId;
	public String getDeliveryId() {
		return deliveryId;
	}
	public void setDeliveryId(String deliveryId) {
		this.deliveryId = deliveryId;
	}
	public Location getPickup() {
		return pickup;
	}
	public void setPickup(Location pickup) {
		this.pickup = pickup;
	}
	public Location getDropoff() {
		return dropoff;
	}
	public void setDropoff(Location dropoff) {
		this.dropoff = dropoff;
	}
	public List<PackageDetail> getPackageDetails() {
		return packageDetails;
	}
	public void setPackageDetails(List<PackageDetail> packageDetails) {
		this.packageDetails = packageDetails;
	}
	public boolean isExpedited() {
		return expedited;
	}
	public void setExpedited(boolean expedited) {
		this.expedited = expedited;
	}
	public DroneDelivery(String deliveryId, Location pickup, Location dropoff, List<PackageDetail> packageDetails,
			boolean expedited) {
		super();
		this.deliveryId = deliveryId;
		this.pickup = pickup;
		this.dropoff = dropoff;
		this.packageDetails = packageDetails;
		this.expedited = expedited;
	}
	public DroneDelivery() {
		super();
	}
	private Location pickup;
	private Location dropoff;
	private List<PackageDetail> packageDetails;
	private boolean expedited;
}
