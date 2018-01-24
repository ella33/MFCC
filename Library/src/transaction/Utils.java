package transaction;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by Elev on 05.02.2017.
 */
public class Utils {

    public static Transaction getTransaction(int id, List<Transaction> transactions){
        for(Transaction transaction : transactions){
            if(transaction.getId() == id){
                return transaction;
            }
        }
        return null;
    }

    public static List<Lock> getLocksForTable(String table, List<Lock> locks){
        List<Lock> locksForTable = new ArrayList<>();
        for(Lock lock : locks){
            if(lock.getTable().equals(table)){
                locksForTable.add(lock);
            }
        }
        return locksForTable;
    }

    public static List<Lock> getLocksForTransaction(Transaction transaction, List<Lock> locks){
        List<Lock> locksForTransaction = new ArrayList<>();
        for(Lock lock : locks){
            if(lock.getTransaction().equals(transaction)){
                locksForTransaction.add(lock);
            }
        }
        return locksForTransaction;
    }
}
