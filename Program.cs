const string DELIMETER_1 = ",";
const string DELIMETER_2 = "|";

string[] STATUSES =   {"Open", "Reopened", "Resolved", "Closed" };
string[] PRIORITIES = {"Low", "Medium", "High", "Urgent", "Emergency"};

string readWriteFilePath = "Tickets.csv";
int lineNumTracker = 0;

string optionsSelector(string[] options){
    string userInput;
    int selectedNumber = -1;
    do{
        Console.WriteLine("Please select an option from the following...");
        for(int i = 1; i <= options.Length; i++){
            Console.WriteLine("  "+i+") "+options[i-1]);
        }
        Console.Write("Please enter a option from the list: ");
        userInput = Console.ReadLine();
    }while(!int.TryParse(userInput, out selectedNumber) || selectedNumber < 1 || selectedNumber > options.Length);
    return options[selectedNumber-1];
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
    addLine += optionsSelector(STATUSES)+DELIMETER_1;
    Console.WriteLine(" Select the priority of the ticket ");
    addLine += optionsSelector(PRIORITIES)+DELIMETER_1;
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