// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

package com.fabrikam.dronedelivery.dronescheduler.models;

public class PackageDetail {
	private String id;
	private PackageSize size;
	public String getId() {
		return id;
	}
	public void setId(String id) {
		this.id = id;
	}
	public PackageSize getSize() {
		return size;
	}
	public PackageDetail(String id, PackageSize size) {
		super();
		this.id = id;
		this.size = size;
	}
	public void setSize(PackageSize size) {
		this.size = size;
	}
	public PackageDetail() {
		super();
	}
}
