#include <iostream>
#include <stdio.h>
#include <string.h> 

int main(int argc, char *argv[])
{
    const std::string helpString = "-c [username] [email (connected to your remote git account)] Create new account\n"
    "-u Current use account\n"
    "-s [account id] Set current account\n"
    "-l List all accounts\n"
    "-i [file path] Import accounts from *.gam file\n"
    "-e [account id] Edit user\n"
    "-gamLoc Get .gam file location\n"
    "-h Help";
    
    //check for help
    if(argc > 1) 
    {
        std::string firstArg(argv[1]);

        if(firstArg == "-h") { std::cout << helpString << std::endl; }
        
    }

    system("pause");
    return 0;
}

void CreateAccount(int argc, char *arguments[])
{
    if(argc < 4) { std::cout << "Please provide a username and an email connected to your remote git account" << std::endl; return; }

    //generate ssh key
    
    //add ssh key to agent
    //print public key
    //save account to .gam file
}