namespace EdgeDB.CLI;

interface ICommand
{
    Task ExecuteAsync();
}