\# RaceDay Data Dictionary



A reference for every table and field in the RaceDay database. See `RaceDay\_ERD.png` for the visual diagram and `RaceDay\_Database.sql` for the full script.



\## Users



Base table for anyone with a login.



| Field | Type | Notes |

|---|---|---|

| UserID | INT | Primary key, auto-increments |

| FirstName | VARCHAR(50) | Required |

| LastName | VARCHAR(50) | Required |

| Email | VARCHAR(100) | Required, must be unique |

| Password | VARCHAR(255) | Required |



\## Organisers



Specialisation of Users — a user who can create and manage events.



| Field | Type | Notes |

|---|---|---|

| OrganiserID | INT | Primary key |

| UserID | INT | Foreign key to Users, unique (one user = at most one organiser record) |



\## Participants



Specialisation of Users — a user who can enrol in events.



| Field | Type | Notes |

|---|---|---|

| ParticipantID | INT | Primary key |

| UserID | INT | Foreign key to Users, unique |



\## Events



Created and managed by an Organiser.



| Field | Type | Notes |

|---|---|---|

| EventID | INT | Primary key, auto-increments |

| EventName | VARCHAR(100) | Required |

| EventDate | DATE | Required |

| Location | VARCHAR(150) | Required |

| RouteDescription | VARCHAR(255) | Optional; basic route info as a plain field (live route/weather data is fetched externally, not stored) |

| OrganiserID | INT | Foreign key to Organisers |



\## Categories



A race category (e.g. "10km", "Half Marathon") belonging to one event.



| Field | Type | Notes |

|---|---|---|

| CategoryID | INT | Primary key, auto-increments |

| CategoryName | VARCHAR(50) | Required |

| EventID | INT | Foreign key to Events |



\## Enrolments



Junction table linking Participants to Events and Categories (resolves the many-to-many relationship between participants and events).



| Field | Type | Notes |

|---|---|---|

| EnrolmentID | INT | Primary key, auto-increments |

| ParticipantID | INT | Foreign key to Participants |

| EventID | INT | Foreign key to Events |

| CategoryID | INT | Foreign key to Categories |

| EnrolmentDate | DATE | Defaults to today's date if not provided |



\## Results



A participant's result for one enrolment, recorded by the organiser.



| Field | Type | Notes |

|---|---|---|

| ResultID | INT | Primary key, auto-increments |

| EnrolmentID | INT | Foreign key to Enrolments, unique (one result per enrolment) |

| FinishTime | VARCHAR(20) | e.g. "01:52:30" |

| Position | INT | Finishing position within the category |

