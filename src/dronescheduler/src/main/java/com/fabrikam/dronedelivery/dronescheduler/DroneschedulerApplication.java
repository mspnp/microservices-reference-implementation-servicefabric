// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

package com.fabrikam.dronedelivery.dronescheduler;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class DroneschedulerApplication {

    private static final Logger log = LoggerFactory.getLogger(DroneschedulerApplication.class);

	public static void main(String[] args) {
		SpringApplication.run(DroneschedulerApplication.class, args);
		log.info("Drone scheduer service started");
	}

}

