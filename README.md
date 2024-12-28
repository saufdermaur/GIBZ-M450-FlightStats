Workflow:

1. Start Application and check if there are any Airports
	1.1. If no Airports, send Request to backend to start fetching Airports (Start Selenium, fetch and write to DB)
		1.2 When finished, proceed
	1.2. If Airports exist, proceed
2. Allow user to define interval (store to db)
3. User can set new trackable flight: Set Origin, Set Destination, Date and Time
	3.1 Fetch Data for the specified input => dont save anything to db yet => return the available flights to the user
	3.2 user selects the flight that he wants to track => save that flight to the db
4. Let magic (selenium) happen 
	4.1 user specified interval gets triggered
	4.2 start background worker 
	4.3 loop over flights in the db (on observe)
	4.1 search for the flight on the specified interval
	4.2 if flight found (through selenium) get it's data and store to db
5. when flight(s) have been tracked, display that data for flight is available 
6. when the flight gets clicked, let the user make requests on the flight