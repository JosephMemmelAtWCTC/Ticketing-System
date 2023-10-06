using NLog;

const string DELIMETER_1 = ",";
const string DELIMETER_2 = "|";


UInt64 lastId = 0;//Should never be negative, but not uint for allowing -1 for error checking
int lineNumTracker = 0;

const bool IS_UNIX = true;

string loggerPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "nlog.config";
string readWriteFilePath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "Tickets.csv";

// create instance of Logger
NLog.Logger logger = LogManager.Setup().LoadConfigurationFromFile(loggerPath).GetCurrentClassLogger();

logger.Info("Main program is running and log mager is started, program is running on a " + (IS_UNIX ? "" : "non-") + "unix-based device.");

// string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies_No_Filter),
//                                         enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies_Filter),
//                                         enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Movies),
//                                         enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit)};


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


Console.WriteLine("Welcome to the 'Ticketing System', please wait while we set things up for you...");

// Attempt to open file
if(System.IO.File.Exists(readWriteFilePath)){
    StreamReader sr = new StreamReader(readWriteFilePath);
    while (!sr.EndOfStream){
        string line = sr.ReadLine();
        // TODO: Count priorties?
        lineNumTracker++;
    }
    sr.Close();
    Console.WriteLine("There are ({0}) tickets on file.", lineNumTracker);
    do{
        Console.WriteLine("---Options---");
        Console.WriteLine(" 1) Enter new ticket");
        Console.WriteLine(" 2) View all tickets");
        Console.Write("Please enter your choice or 'q' to quit: ");

        string userInput = Console.ReadLine();
        if(userInput.Length == 0){
            userInput=" ";
        }
        char selectChar = userInput[0];
        selectChar = Char.ToUpper(selectChar);
    
        switch (selectChar)
        {
            case '1':
                string addLine = createTicketLine();
                StreamWriter sw = new StreamWriter(readWriteFilePath, true);
                sw.WriteLine(addLine);
                sw.Close();
            break;
            case '2':
                printTicketList();
            break;
            case 'Q':
                return;
            default:
                Console.WriteLine("Sorry but '"+selectChar+"' is not a valid option. Please try again.");
            break;
        }

    }while(true);
// 1,This is a bug ticket,Open,High,Drew Kjell,Jane Doe, Drew Kjell|John Smith|Bill Jones


}else{
    Console.WriteLine("The file, '{0}' was not found.", readWriteFilePath);
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


// vvv UNUM STUFF vvv

// string enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS mainMenuEnum)
// {
//     return mainMenuEnum switch
//     {
//         MAIN_MENU_OPTIONS.Exit => "Quit program",
//         MAIN_MENU_OPTIONS.View_Movies_No_Filter => $"View movies on file in order (display max ammount is {PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT:N0})",
//         MAIN_MENU_OPTIONS.View_Movies_Filter => $"Filter movies on file",
//         MAIN_MENU_OPTIONS.Add_Movies => "Add movies to file",
//         _ => "ERROR"
//     };
// }
// string Ticket.GetEnumStatusFromString(FILTER_MENU_OPTIONS filterMenuEnum)
// {
//     return filterMenuEnum switch
//     {
//         FILTER_MENU_OPTIONS.Exit => "Quit Filtering",
//         FILTER_MENU_OPTIONS.Year => "By year",
//         FILTER_MENU_OPTIONS.Title => "By title",
//         FILTER_MENU_OPTIONS.Genre => "By genre",
//         _ => "ERROR"
//     };
// }