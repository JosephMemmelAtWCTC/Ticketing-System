using NLog;

const string DELIMETER_1 = ",";
const string DELIMETER_2 = "|";
const string START_END_SUMMARY_WITH_DELIMETER1_INDICATOR = "\"";

const bool REMOVE_DUPLICATES = true;
const int PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT = 1_000; //Tested, >~ 1,000 line before removal, use int.MaxValue for infinity, int's length is max for used lists


UInt64 lastId = 0;//Should never be negative, but not uint for allowing -1 for error checking
int lineNumTracker = 0;

const bool IS_UNIX = true;

string loggerPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "nlog.config";
string readWriteFilePath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "Tickets.csv";

// create instance of Logger
NLog.Logger logger = LogManager.Setup().LoadConfigurationFromFile(loggerPath).GetCurrentClassLogger();

logger.Info("Main program is running and log mager is started, program is running on a " + (IS_UNIX ? "" : "non-") + "unix-based device.");

List<int> ticketHashes = new List<int>();//Store data hashes for speed, stored centrally. TODO: Move out if needed

string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_No_Filter),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_Filter),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Ticket),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit)};

string[] TICKET_STATUSES_IN_ORDER = {Ticket.StatusesEnumToString(Ticket.STATUSES.OPEN),
                                     Ticket.StatusesEnumToString(Ticket.STATUSES.REOPENDED),
                                     Ticket.StatusesEnumToString(Ticket.STATUSES.RESOLVED),
                                     Ticket.StatusesEnumToString(Ticket.STATUSES.CLOSED)};

string[] TICKET_PRIORITIES_IN_ORDER = {Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.LOW),
                                       Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.MEDIUM),
                                       Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.HIGH),
                                       Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.Urgent),
                                       Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.EMERGENCY)};


string optionsSelector(string[] options)
{
    string userInput;
    int selectedNumber;
    bool userInputWasImproper = true;
    List<int> cleanedListIndexs = new List<int> { };
    string optionsTextAsStr = ""; //So only created once. Requires change if adjustable width is added

    for (int i = 0; i < options.Length; i++)
    {
        // options[i] = options[i].Trim();//Don't trim so when used, spaces can be used to do spaceing
        if (options[i] != null && options[i].Replace(" ", "").Length > 0)
        {//Ensure that not empty or null
            cleanedListIndexs.Add(i);//Add index to list
            optionsTextAsStr = $"{optionsTextAsStr}\n{string.Format($" {{0,{options.Length.ToString().Length}}}) {{1}}", cleanedListIndexs.Count(), options[i])}";//Have to use this as it prevents the constents requirment.
        }
    }
    optionsTextAsStr = optionsTextAsStr.Substring(1); //Remove first \n

    // Seprate from rest by adding a blank line
    Console.WriteLine();
    do
    {
        Console.WriteLine("Please select an option from the following...");
        Console.WriteLine(optionsTextAsStr);
        Console.Write("Please enter an option from the list: ");
        userInput = Console.ReadLine().Trim();

        //TODO: Move to switch without breaks instead of ifs or if-elses?
        if (!int.TryParse(userInput, out selectedNumber))
        {// User response was not a integer
            logger.Error("Your selector choice was not a integer, please try again.");
        }
        else if (selectedNumber < 1 || selectedNumber > cleanedListIndexs.Count()) //Is count because text input index starts at 1
        {// User response was out of bounds
            logger.Error($"Your selector choice was not within bounds, please try again. (Range is 1-{cleanedListIndexs.Count()})");
        }
        else
        {
            userInputWasImproper = false;
        }
    } while (userInputWasImproper);
    // Seprate from rest by adding a blank line
    Console.WriteLine();
    return options[cleanedListIndexs[selectedNumber - 1]];
}

string userCreatedStringObtainer(string message, int minimunCharactersAllowed, bool showMinimum, bool keepRaw){
    if(minimunCharactersAllowed < 0){
        minimunCharactersAllowed = 0;
    }
    string userInput = null;

    do{
        Console.Write($"\n{message}{(showMinimum?$" (must contain at least {minimunCharactersAllowed} characters)":"")}: ");
        userInput = Console.ReadLine().ToString();
        if(!keepRaw){
            userInput = userInput.Trim();
        }
        if(minimunCharactersAllowed > 0 && userInput.Length == 0){
           userInput = null;
            logger.Warn($"Entered input was blank, input not allowed to be empty, please try again.");
        }else if(userInput.Length < minimunCharactersAllowed){
            userInput = null;
            logger.Warn($"Entered input was too short, it must be at least {minimunCharactersAllowed} characters long, please try again.");
        }
    }while(userInput == null);
    
    return userInput;
}


List<Ticket> tickets = buildTicketListFromFile(readWriteFilePath);


// Attempt to open file
if (System.IO.File.Exists(readWriteFilePath))
{
    StreamReader sr = new StreamReader(readWriteFilePath);
    while (!sr.EndOfStream)
    {
        string line = sr.ReadLine();
        // TODO: Count priorties?
        lineNumTracker++;
    }
    sr.Close();
    Console.WriteLine($"There are ({lineNumTracker}) ticket{(lineNumTracker == 1 ? "" : "s")} on file.");
    do
    {
        // TODO: Move to switch with integer of place value and also make not relient on index by switching to enum for efficiency
        string menuCheckCommand = optionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

        if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit))
        {//If user intends to exit the program
            logger.Info("Program quiting...");
            return;
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_No_Filter))
        {
            // presentListRange(tickets);
            printTicketList(tickets);
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_Filter))
        {

        }
        else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Ticket))
        {
            Ticket newTicket = createNewTicket();
            if (!REMOVE_DUPLICATES || checkTicketIsNotDuplicate(ticketHashes, newTicket))
            {
                // attemptToSaveTicket(newTicket);@@@
            }
            else
            {
                logger.Warn($"Dupliate ticket summary record on ticket \"{newTicket.Summary}\" with id \"{newTicket.Id}\". Not adding to records.");
            }
            // StreamWriter sw = new StreamWriter(readWriteFilePath, true);

            //TODO: Inform user that ticket was created and added    

        }
        else
        {
            logger.Fatal("Somehow, menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
        }

    } while (true);
    // 1,This is a bug ticket,Open,High,Drew Kjell,Jane Doe, Drew Kjell|John Smith|Bill Jones


}
else
{
    Console.WriteLine($"The file, '{readWriteFilePath}' was not found.");
}

void printTicketList(List<Ticket> displayTickets)
{
    Console.WriteLine("+----------+--------+----------+--------------------+--------------------+----------------------------------------+--------------------------------------------+");
    Console.WriteLine("| TicketID | Status | Priority |     Submitter      |      Asigned       |              Watching                  |                  Summary                   |");
    Console.WriteLine("+----------+--------+----------+--------------------+--------------------+----------------------------------------+--------------------------------------------+");
    StreamReader sr = new StreamReader(readWriteFilePath);

    foreach (Ticket ticket in displayTickets)
    {
        string ticketId, summary, status, priority, submitter, asigned, watching;
        ticketId = ticket.Id.ToString();
        summary = ticket.Summary;
        status = Ticket.StatusesEnumToString(ticket.Status);
        priority = Ticket.PrioritiesEnumToString(ticket.Priority);
        submitter = ticket.Submitter;
        asigned = ticket.Asigned;
        watching = ticket.Watching.Aggregate((current, next) => current + ", " + next);
        Console.WriteLine("| " +
            String.Format("{0, -9}", ticketId) + "|" +
            String.Format("{0, -8}", status) + "|" +
            String.Format("{0, -9}", priority) + " |" +
            String.Format("{0, -20}", submitter) + "|" +
            String.Format("{0, -20}", asigned) + "|" +
            String.Format("{0, -40}", watching) + "|" +
            String.Format("{0, -44}", summary) + "|"
        );
    }

    Console.WriteLine("+----------+--------+----------+--------------------+--------------------+----------------------------------------+--------------------------------------------+");
}

string createTicketLine()
{

    // Place in order of {TicketID, Summary, Status, Priority, Submitter, Assigned, Watching}
    Console.WriteLine("Creating a new ticket...");
    string addLine = (++lineNumTracker) + DELIMETER_1;
    Console.Write(" Enter a summary of the ticket: ");
    addLine += Console.ReadLine() + DELIMETER_1;
    Console.WriteLine(" Select the status of the ticket ");
    addLine += optionsSelector(TICKET_STATUSES_IN_ORDER) + DELIMETER_1;
    Console.WriteLine(" Select the priority of the ticket ");
    addLine += optionsSelector(TICKET_PRIORITIES_IN_ORDER) + DELIMETER_1;
    Console.Write(" Enter the submitter of the ticket: ");
    string nameInput = Console.ReadLine();//TODO: Move to format name method and handle extra cases
    if (nameInput.Length > 0) { nameInput = Char.ToUpper(nameInput[0]) + nameInput.Substring(1); }
    addLine += nameInput + DELIMETER_1;
    Console.Write(" Enter the person assigned to the ticket: ");
    nameInput = Console.ReadLine();
    if (nameInput.Length > 0) { nameInput = Char.ToUpper(nameInput[0]) + nameInput.Substring(1); }
    addLine += nameInput + DELIMETER_1;
    do
    {
        Console.Write(" Enter the name of a person watching the ticket or leave blank to compleate the ticket: ");
        nameInput = Console.ReadLine();
        if (nameInput.Length == 0) { break; }
        nameInput = Char.ToUpper(nameInput[0]) + nameInput.Substring(1);
        addLine += nameInput + DELIMETER_2;
    } while (true);
    addLine = addLine.Substring(0, addLine.Length - 1); //Removes last (an extra) DELIMETER_2
    return addLine;
}


Ticket createNewTicket()
{
    string userInputRaw;
    UInt64 userChoosenInteger;

    // Ticket summary
    string ticketSummary = userCreatedStringObtainer("Please enter the summary of the new ticket", 5, true, false);

    UInt64 newId = lastId + 1; //Assume last record id is not out of order, avoid using auto id for placing with repeat id's that may have existed before but then were removed. Option avaiable if manually entering id.
    do
    {
        Console.Write($"To use ticket id \"{newId}\", leave blank, else enter integer now: ");
        userInputRaw = Console.ReadLine().Trim();
        if (UInt64.TryParse(userInputRaw, out userChoosenInteger) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
        {
            if (userInputRaw.Length == 0 || userChoosenInteger == newId)
            { //Skip check if using auto id, manually typed or by entering blank
                userChoosenInteger = newId;
                lastId++;//Increment last id
            }
            else if (userChoosenInteger <= 0)
            {
                logger.Error("Your choosen id choice was not a positive integer above 0, please try again.");
                userChoosenInteger = 0;
            }
            else
            {
                // TODO: Make more efficent
                foreach (Ticket ticket in tickets) // Check if id is already used
                {
                    if (ticket.Id == userChoosenInteger)
                    {
                        logger.Error("Your choosen id is already in use, please try again.");
                        userChoosenInteger = 0;
                    }
                }
            }
        }
        else
        {
            //User response was not a integer
            logger.Error("Your choosen id choice was not a integer, please try again.");
            userChoosenInteger = 0; //Was not an integer
        }
    } while (userChoosenInteger == 0);

    UInt64 ticketId = userChoosenInteger;
    Ticket.STATUSES selectedStatus = Ticket.GetEnumStatusFromString(optionsSelector(TICKET_STATUSES_IN_ORDER));
    Ticket.PRIORITIES selectedPriority = Ticket.GetEnumPriorityFromString(optionsSelector(TICKET_PRIORITIES_IN_ORDER));
    string selectedSubmitter = userCreatedStringObtainer("Please enter the name of this ticket's submitter", 1, true, false);
    string selectedAssigned = userCreatedStringObtainer("Enter the person assigned to the ticket", 1, true, false);
    List<string> selectedWatchers = new List<string>(){ };
    do{
        selectedWatchers.Add(userCreatedStringObtainer("Enter the name of a person watching the ticket or leave blank to compleate the ticket", 0, true, true));
    }while(selectedWatchers.Last().Length != 0);
    selectedWatchers.RemoveAt(selectedWatchers.Count()-1);
    

    return new Ticket(ticketId, ticketSummary, selectedStatus, selectedPriority, selectedSubmitter, selectedAssigned, selectedWatchers.ToArray());
}

bool checkTicketIsNotDuplicate(List<int> checkAgainstHashes, Ticket checkTicket)
{
    //Check hashtable for existing combination and add
    int ticketHash = checkTicket.GetHashCode();
    return checkAgainstHashes.Contains(ticketHash);
}

List<Ticket> buildTicketListFromFile(string dataPath)
{
    List<Ticket> ticketsInFile = new List<Ticket>();

    // Info for tracking
    uint lineNumber = 1;//Should never be negative, so uint

    // ALL TERMINATORS
    if (!System.IO.File.Exists(dataPath))
    {
        logger.Fatal($"The file, '{dataPath}' was not found.");
        // throw new FileNotFoundException();
        return null;
    }
    // Take care of the rest of at this point all unknown filesystem errors (not accessable, ect.)
    StreamReader sr;
    try
    {
        sr = new StreamReader(dataPath);
    }
    catch (Exception ex)
    {
        logger.Fatal(ex.Message);
        // throw new Exception($"Problem using file at \"{dataPath}\"");
        return null;
    }

    while (!sr.EndOfStream)
    {
        bool recordIsBroken = false;
        string line = sr.ReadLine().Trim();
        string[] ticketParts = line.Split(DELIMETER_1);
        if(ticketParts.Length > 7){ //More commas, need to merge inside summary, does not require quotation marks to seprate fields. Can change in future if requirements change
            string merged = ticketParts[1..(ticketParts.Length-5)].Aggregate((current, next) => $"{current}{DELIMETER_1}{next}"); //Put commas back in
            ticketParts[1] = merged;
            short counter2 = 0;
            for(int i = ticketParts.Length-5; i < ticketParts.Length; i++)
            {
                ticketParts[2+counter2++] = ticketParts[i];
            }
            ticketParts = ticketParts[0..7];
            Console.WriteLine(ticketParts.Aggregate((current, next) => $"{current}--{next}"));
        }
        Console.WriteLine("ticketParts = "+ticketParts.Aggregate((current, next) => current + " ][ " + next));

        if (ticketParts.Length < 7)
        {
            logger.Error($"Broken ticket record on line #{lineNumber} (\"{line}\"). Not enough arguments provided on line. Must have an id, a summary, a status, a priorty, a submitter, an asigned person and watcher(s).");
            recordIsBroken = true;
        }
        else if (ticketParts.Length > 7)
        {
            logger.Error($"Broken ticket record on line #{lineNumber} (\"{line}\"). Too many arguments provided on line. Must have an id, a summary, a status, a priorty, a submitter, an asigned person and watcher(s).");
            recordIsBroken = true;
        }
        if (!UInt64.TryParse(ticketParts[0], out UInt64 ticketId))
        {
            logger.Error($"Broken ticket record on line #{lineNumber} (\"{line}\"). Ticket id is not a integer. Ticket id must be a integer.");
            recordIsBroken = true;
        }
        string ticketSummary = "";
        if (!recordIsBroken)
        {
            ticketSummary = ticketParts[1].Trim();
            if (ticketSummary.Length == 0 || ticketSummary == " ")
            {
                logger.Error($"Broken ticket record on line #{lineNumber} (\"{line}\"). Ticket summary is empty. Ticket summary cannot be blank or empty.");
                recordIsBroken = true;
            }
        }
        string ticketSubmitter = "";
        if (!recordIsBroken)
        {
            ticketSubmitter = ticketParts[2].Trim();
            if (ticketSummary.Length == 0 || ticketSummary == " ")
            {
                logger.Error($"Broken ticket record on line #{lineNumber} (\"{line}\"). Ticket summary is empty. Ticket summary cannot be blank or empty.");
                recordIsBroken = true;
            }
        }

        if (!recordIsBroken)
        {
            Ticket ticket = new Ticket(ticketId, ticketSummary, ticketParts[2], ticketParts[3], ticketParts[4], ticketParts[5], new string[]{ticketParts[6]});
            if (REMOVE_DUPLICATES)
            {
                //Check hashtable for existing combination and add
                int ticketSummaryHash = ticket.GetHashCode();
                if (ticketHashes.Contains(ticketSummaryHash))
                {
                    logger.Warn($"Dupliate ticket record on ticket \"{ticket.Summary.Replace("\"", "")}\" with id \"{ticket.Id}\". Not including in results.");//TODO: Update line
                }
                else
                {
                    ticketsInFile.Add(ticket);
                    ticketHashes.Add(ticketSummaryHash);
                }
            }
            else
            {
                ticketsInFile.Add(ticket);
            }

            // Console.WriteLine(ticket);
        }else{
            Console.WriteLine("FAILED!!!");
        }

        // Update helpers
        lineNumber++;
        lastId = Math.Max(lastId, ticketId);
    }
    sr.Close();
    return ticketsInFile;
}


// vvv UNUM STUFF vvv

string enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS mainMenuEnum)
{
    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.View_Tickets_No_Filter => $"View tickets on file in order (display max ammount is {PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT:N0})",
        MAIN_MENU_OPTIONS.View_Tickets_Filter => $"Filter tickets on file",
        MAIN_MENU_OPTIONS.Add_Ticket => "Add ticket to file",
        _ => "ERROR"
    };
}
string enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS filterMenuEnum)
{
    return filterMenuEnum switch
    {
        FILTER_MENU_OPTIONS.Exit => "Quit Filtering",
        FILTER_MENU_OPTIONS.Year => "By year",
        FILTER_MENU_OPTIONS.Summary => "By summary",
        FILTER_MENU_OPTIONS.Genre => "By genre",
        _ => "ERROR"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    View_Tickets_No_Filter,
    View_Tickets_Filter,
    Add_Ticket
}

public enum FILTER_MENU_OPTIONS
{
    Exit,
    Year,
    Summary,
    Genre
    // Id
}