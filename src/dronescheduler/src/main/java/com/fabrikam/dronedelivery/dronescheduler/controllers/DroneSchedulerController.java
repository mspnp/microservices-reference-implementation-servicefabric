// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

package com.fabrikam.dronedelivery.dronescheduler.controllers;

import java.util.UUID;
import java.util.concurrent.CompletableFuture;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.bind.annotation.RestController;

import com.fabrikam.dronedelivery.dronescheduler.models.DroneDelivery;

@RestController
public class DroneSchedulerController {

	private final static Logger log = LoggerFactory.getLogger(DroneSchedulerController.class);
	private static final String template = "AssignedDroneId:%s";
	
	@RequestMapping(value="/api/{id}", method = RequestMethod.PUT)
	@ResponseBody
	public CompletableFuture<ResponseEntity<String>> getDroneIdAsync(@RequestBody DroneDelivery droneDelivery, @PathVariable("id") String id) {
		log.info("In getDroneIdAsync method with delivery id {}", id);
		String randomUUIDString = UUID.randomUUID().toString();
		return CompletableFuture.completedFuture(new ResponseEntity<>(String.format(template, randomUUIDString), HttpStatus.CREATED));
	}
}
