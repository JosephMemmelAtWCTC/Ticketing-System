using NLog;

const string DELIMETER_1 = ",";
const string DELIMETER_2 = "|";
const string START_END_TITLE_WITH_DELIMETER1_INDICATOR = "\"";

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

List<int> ticketsTitleYearHash = new List<int>();//Store data hashes for speed, stored centrally. TODO: Move out if needed

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
    List<int> cleanedListIndexs = new List<int> {};
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

List<Ticket> tickets = buildTicketListFromFile(readWriteFilePath);


// Attempt to open file
if(System.IO.File.Exists(readWriteFilePath)){
    StreamReader sr = new StreamReader(readWriteFilePath);
    while (!sr.EndOfStream){
        string line = sr.ReadLine();
        // TODO: Count priorties?
        lineNumTracker++;
    }
    sr.Close();
    Console.WriteLine($"There are ({lineNumTracker}) ticket{(lineNumTracker==1 ? "":"s")} on file.");
    do{
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
            printTicketList();
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_Filter))
        {

        }
        else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Ticket))
        {
            Ticket newTicket = createNewTicket();
            // StreamWriter sw = new StreamWriter(readWriteFilePath, true);

            // Inform user that ticket was created and added    

        }
        else
        {
            logger.Fatal("Somehow, menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
        }

    }while(true);
// 1,This is a bug ticket,Open,High,Drew Kjell,Jane Doe, Drew Kjell|John Smith|Bill Jones


}else{
    Console.WriteLine($"The file, '{readWriteFilePath}' was not found.");
}

void printTicketList(){
    if(System.IO.File.Exists(readWriteFilePath)){
        Console.WriteLine("+----------+--------+----------+--------------------+--------------------+----------------------------------------+--------------------------------------------+");
        Console.WriteLine("| TicketID | Status | Priority |     Submitter      |      Asigned       |              Watching                  |                  Summary                   |");
        Console.WriteLine("+----------+--------+----------+--------------------+--------------------+----------------------------------------+--------------------------------------------+");
        StreamReader sr = new StreamReader(readWriteFilePath);
        
        while (!sr.EndOfStream){
            string line = sr.ReadLine();
            string[] elements = line.Split(DELIMETER_1);
            if(elements.Length == 6){ //There was not watcher asigned
                elements[6] = "";
            }
            string ticketID, summary, status, priority, submitter, asigned, watching;
            ticketID = elements[0];
            summary = elements[1];
            status = elements[2];
            priority = elements[3];
            submitter = elements[4];
            asigned = elements[5];
            watching = elements[6];
            Console.WriteLine("| "+
                String.Format("{0, -9}",ticketID)+"|"+
                String.Format("{0, -8}",status)+"|"+
                String.Format("{0, -9}",priority)+" |"+
                String.Format("{0, -20}",submitter)+"|"+
                String.Format("{0, -20}",asigned)+"|"+
                String.Format("{0, -40}",watching.Replace(DELIMETER_2,", "))+"|"+
                String.Format("{0, -44}",summary)+"|"
            );
        }
        Console.WriteLine("+----------+--------+----------+--------------------+--------------------+----------------------------------------+--------------------------------------------+");
        sr.Close();
    }else{
        Console.WriteLine("The file, '{0}' was not found.", readWriteFilePath);
    }
}

string createTicketLine(){

    // Place in order of {TicketID, Summary, Status, Priority, Submitter, Assigned, Watching}
    Console.WriteLine("Creating a new ticket...");
    string addLine = (++lineNumTracker)+DELIMETER_1;
    Console.Write(" Enter a summary of the ticket: ");
    addLine += Console.ReadLine()+DELIMETER_1;
    Console.WriteLine(" Select the status of the ticket ");
    addLine += optionsSelector(TICKET_STATUSES_IN_ORDER)+DELIMETER_1;
    Console.WriteLine(" Select the priority of the ticket ");
    addLine += optionsSelector(TICKET_PRIORITIES_IN_ORDER)+DELIMETER_1;
    Console.Write(" Enter the submitter of the ticket: ");
    string nameInput = Console.ReadLine();//TODO: Move to format name method and handle extra cases
    if(nameInput.Length > 0){ nameInput = Char.ToUpper(nameInput[0])+nameInput.Substring(1); }
    addLine += nameInput+DELIMETER_1;
    Console.Write(" Enter the person assigned to the ticket: ");
    nameInput = Console.ReadLine();
    if(nameInput.Length > 0){ nameInput = Char.ToUpper(nameInput[0])+nameInput.Substring(1); }
    addLine += nameInput+DELIMETER_1;
    do{
        Console.Write(" Enter the name of a person watching the ticket or leave blank to compleate the ticket: ");
        nameInput = Console.ReadLine();
        if(nameInput.Length == 0){ break; }
        nameInput = Char.ToUpper(nameInput[0])+nameInput.Substring(1);
        addLine += nameInput+DELIMETER_2;
    }while(true);
    addLine = addLine.Substring(0,addLine.Length-1); //Removes last (an extra) DELIMETER_2
    return addLine;
}


Ticket createNewTicket(){
    string userInputRaw;
    UInt64 userChoosenInteger;

    // Ticket title
    string ticketTitle;
    bool ticketTitleFailed = true;
    do
    {
        Console.Write("Please enter the title of the new ticket: ");
        userInputRaw = Console.ReadLine().Trim();
        ticketTitle = userInputRaw.Trim();
        if (ticketTitle.Length == 0)
        {
            logger.Error("Ticket title cannot be left blank, please try again.");
        }
        else
        {
            ticketTitleFailed = false;
        }

        if (userInputRaw.Contains(","))
        {
            ticketTitle = $"\"{userInputRaw}\"";
        }
    } while (ticketTitleFailed);

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

    Ticket.STATUSES selectedStatus = Ticket.GetEnumStatusFromString(optionsSelector(TICKET_STATUSES_IN_ORDER));
    Ticket.PRIORITIES selectedPriority = Ticket.GetEnumPriorityFromString(optionsSelector(TICKET_PRIORITIES_IN_ORDER));


    UInt64 ticketId = userChoosenInteger;

    //Write the record
    // TODO: ensue no errors with SW!
    if (ticketTitle.EndsWith("\""))
    {//Merge year with title (some exisiting records do not have a year, but going forward, all should so it's included here)
        ticketTitle = $"{ticketTitle.Substring(0, ticketTitle.Length - 2)}";
    }
    else
    {
        ticketTitle = $"{ticketTitle}";
    }
    
    return new Ticket(ticketId, ticketTitle, selectedStatus, selectedPriority);
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
        bool recordIsBroken = true;
        string line = sr.ReadLine();
        // string[] ticketParts = line.Substring(0, line.IndexOf(DELIMETER_1));
        string[] ticketParts = line.Split(DELIMETER_1);
        if (ticketParts.Length > 3 && (line.Substring(line.IndexOf(DELIMETER_1)).Split(START_END_TITLE_WITH_DELIMETER1_INDICATOR).Length - 1 >= 2))
        {//Assume first that quotation marks are used to lower
            ushort indexOfFirstDelimeter1 = (ushort)(line.IndexOf(DELIMETER_1) + 1);//Can be ushort as line above makes sure cannot be -1
            ushort indexOfLastDelimeter1 = (ushort)line.Substring(indexOfFirstDelimeter1).LastIndexOf(DELIMETER_1);//Can be ushort as line above makes sure cannot be -1
            ticketParts[1] = line.Substring(indexOfFirstDelimeter1, indexOfLastDelimeter1).Replace(START_END_TITLE_WITH_DELIMETER1_INDICATOR, "");
            ticketParts[2] = ticketParts[ticketParts.Length - 1];//Get last element that was split using delimeter #1
            ticketParts = new string[] { ticketParts[0], ticketParts[1], ticketParts[2] };
        }

        if (ticketParts.Length <= 2)
        {
            logger.Error($"Broken ticket record on line #{lineNumber} (\"{line}\"). Not enough arguments provided on line. Must have a id, a title, and optionally genres.");
        }
        else if (ticketParts.Length > 3)
        {
            logger.Error("ticketParts=" + ticketParts.Length + $"Broken ticket record on line #{lineNumber} (\"{line}\"). Too many arguments provided on line. Must have a id, a title, and optionally genres.");
        }
        else
        {
            recordIsBroken = false;
        }
        if (!UInt64.TryParse(ticketParts[0], out UInt64 ticketId))
        {
            logger.Error($"Broken ticket record on line #{lineNumber} (\"{line}\"). Ticket id is not a integer. Ticket id must be a integer.");
            recordIsBroken = true;
        }
        string ticketTitle = "";
        if (!recordIsBroken)
        {
            ticketTitle = ticketParts[1];
            if (ticketTitle.Length == 0 || ticketTitle == " ")
            {
                logger.Error($"Broken ticket record on line #{lineNumber} (\"{line}\"). Ticket title is empty. Ticket title cannot be blank or empty. !!!!!" + ticketTitle + "!!!!!");
                recordIsBroken = true;
            }
        }

        if (!recordIsBroken)
        {
            string genres = ticketParts[2];
            Ticket ticket = new Ticket(ticketId, ticketTitle, genres, DELIMETER_2);
            if (REMOVE_DUPLICATES)
            {
                //Check hashtable for existing combination and add
                int ticketTitleYearHash = ticket.GetHashCode();
                if (ticketsTitleYearHash.Contains(ticketTitleYearHash))
                {
                    logger.Warn($"Dupliate ticket record on ticket \"{ticket.Title.Replace("\"", "")}\" with id \"{ticket.Id}\". Not including in results.");//TODO: Update line
                }
                else
                {
                    ticketsInFile.Add(ticket);
                    ticketsTitleYearHash.Add(ticketTitleYearHash);
                }
            }
            else
            {
                ticketsInFile.Add(ticket);
            }

            // Console.WriteLine(ticket);
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
        FILTER_MENU_OPTIONS.Title => "By title",
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
    Title,
    Genre
    // Id
}