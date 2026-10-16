\# RaceDay



RaceDay is a race event management system that connects event organisers with participants. Organisers can create and manage running events, while participants can browse events, enrol, and track their performance history.



This repository covers \*\*Part 1\*\* of the project: planning and database design. No API has been built yet — this stage focuses on the entity-relationship design, API endpoint plan, and SQL database.



\## Roles



\### Event Organisers

\- Create and manage events

\- Manage categories associated with events

\- Manage participant results



\### Participants

\- Browse upcoming events

\- Enter (register for) events

\- Track their personal performance history

\- Prepare for race day using live weather and route information



\## Project structure



RaceDay

\- docs/

&#x20; - RaceDay\_ERD.png — Entity-relationship diagram (7 entities)

&#x20; - API\_Endpoint\_Plan.pdf — Full endpoint plan with roles and responses

&#x20; - RaceDay\_Database.sql — Database creation script + sample data

\- .github/workflows/

&#x20; - part1-check.yml — CI check confirming required files exist

\- README.md



\## Database design



The database has 7 entities: Users, Organisers, Participants, Events, Categories, Enrolments, and Results. Organisers and Participants are modelled as specialisations of Users, which lets foreign keys enforce role-specific relationships (e.g. only an Organiser can be linked to an Event) at the database level.



See docs/RaceDay\_ERD.png for the full diagram and docs/RaceDay\_Database.sql for the matching SQL script.



\## CI/CD



A GitHub Actions workflow (.github/workflows/part1-check.yml) runs on every push to main and verifies that the required documentation files are present.



!\[CI status](https://github.com/suwenbyrow/RaceDay1.0/actions/workflows/part1-check.yml/badge.svg)



\## Video walkthrough



\[https://youtu.be/KTR9H3HJSFQ]

\## Part 2: RESTful API



The Part 2 API is in the `RaceDayAPI` folder. It is an ASP.NET Core Web API (.NET 10) that uses Entity Framework Core with SQL Server, built to the endpoint plan in `docs/API\_Endpoint\_Plan.pdf`. Logout (`POST /api/auth/logout`) was added to the plan, because session login needs a way to end the session.



\### Roles



\*\*Organiser\*\*

\- Creates, updates and deletes their own events

\- Adds, renames and deletes categories on their own events

\- Records results for participants enrolled in their own events



\*\*Participant\*\*

\- Views events, categories and event results

\- Enrols in an event (one enrolment per event, in a category of that event) and cancels their own enrolment

\- Views their own enrolments and their own race history



Both roles can register, log in, log out, and view or update their own profile.



\### Authentication



Passwords are stored as salted hashes. Logging in starts a cookie session that holds the user's ID and role. Requests with no session get 401, and requests from the wrong role (or the wrong owner) get 403.



\### Setup and run



1\. Install the .NET 10 SDK and SQL Server Express (with SSMS).

2\. Open `docs/RaceDay\_Database.sql` in SSMS, connect to `localhost\\SQLEXPRESS`, and run it. This creates the `RaceDay` database with sample data.

3\. Check the connection string in `RaceDayAPI/appsettings.json` matches your server name.

4\. In a terminal:

