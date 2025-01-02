Workflow:


1. Allow user to define interval (store to db)
2. User can set new trackable flight: Set Origin, Set Destination, Date and Time
	2.1 Fetch Data for the specified input => dont save anything to db yet => return the available flights to the user
	2.2 user selects the flight that he wants to track => save that flight to the db
3. Let magic (selenium) happen 
	3.1 user specified interval gets triggered
	3.2 start background worker 
	3.3 loop over flights in the db (on observe)
	3.4 search for the flight on the specified interval
	3.5 if flight found (through selenium) get it's data and store to db
4. when flight(s) have been tracked, display that data for flight is available 
5. when the flight gets clicked, let the user make requests on the flight

---

airport data from: https://openflights.org/data


{
  "originId": 1678,
  "destinationId": 1382,
  "flightNumber": "AF 1415",
  "isBeingTracked": true
}


new york: 3797
la: 3484
	