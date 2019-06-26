// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

package com.fabrikam.dronedelivery.dronescheduler;

import static org.assertj.core.api.Assertions.assertThat;
import static org.hamcrest.CoreMatchers.containsString;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.asyncDispatch;

import java.util.ArrayList;
import java.util.List;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.context.junit4.SpringRunner;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;
import org.springframework.test.web.servlet.request.MockHttpServletRequestBuilder;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.result.MockMvcResultHandlers;
import org.springframework.test.web.servlet.result.MockMvcResultMatchers;

import com.fabrikam.dronedelivery.dronescheduler.controllers.DroneSchedulerController;
import com.fabrikam.dronedelivery.dronescheduler.models.DroneDelivery;
import com.fabrikam.dronedelivery.dronescheduler.models.Location;
import com.fabrikam.dronedelivery.dronescheduler.models.PackageDetail;
import com.fabrikam.dronedelivery.dronescheduler.models.PackageSize;
import com.microsoft.applicationinsights.core.dependencies.google.gson.Gson;

@RunWith(SpringRunner.class)
@SpringBootTest
@AutoConfigureMockMvc
public class DroneschedulerApplicationTests {

	@Autowired
	private DroneSchedulerController controller;

	@Autowired
	private MockMvc mockMvc;

	@Test
	public void contextLoads() throws Exception {
		assertThat(controller).isNotNull();
	}

	@Test
	public void givenDroneDelivery_whenPutDeliveryId_thenReturnAssignedDroneId() throws Exception {
		Location dropoff = new Location(0.0, 0.1, 0.2);
		Location pickup = new Location(0.3, 0.4, 0.5);
		PackageDetail packageDetail = new PackageDetail("packageId", PackageSize.Medium);
		List<PackageDetail> packages = new ArrayList<PackageDetail>();
		packages.add(packageDetail);

		DroneDelivery delivery = new DroneDelivery("someDeliveryId", pickup, dropoff, packages, true);
		String jsonRep = getDroneDeliveryInJson(delivery);

		// Build PUT request
		MockHttpServletRequestBuilder builder = MockMvcRequestBuilders.put("/api/" + "someDeliveryId")
				.contentType(MediaType.APPLICATION_JSON_VALUE).accept(MediaType.APPLICATION_JSON)
				.characterEncoding("UTF-8").content(jsonRep);

		MvcResult result = this.mockMvc.perform(builder).andReturn();
		this.mockMvc.perform(asyncDispatch(result)).andExpect(MockMvcResultMatchers.status().isCreated())
				.andExpect(MockMvcResultMatchers.content().string(containsString("AssignedDroneId")))
				.andDo(MockMvcResultHandlers.print());
	}

	private String getDroneDeliveryInJson(DroneDelivery droneDelivery) {
		Gson gson = new Gson();
		return gson.toJson(droneDelivery);
	}

}
