package transaction;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by Elev on 05.02.2017.
 */
public class DeadlockDetector implements Runnable{

    private TransactionManager transactionManager;

    public DeadlockDetector(TransactionManager transactionManager) {
        this.transactionManager = transactionManager;
    }

    @Override
    public void run() {
        while(true){
            List<Transaction> transactionsCopy = new ArrayList<>(transactionManager.getTransactions());
            for(Transaction t1 : transactionsCopy){
                for(Transaction t2 : transactionsCopy){
                    if(!t1.equals(t2)){
                        List<Lock> locksForFirstTransaction = Utils.getLocksForTransaction(t1, transactionManager.getLocks());
                        List<Lock> locksForSecondTransaction = Utils.getLocksForTransaction(t2, transactionManager.getLocks());
                        List<Lock> waitLocksForFirstTransaction = Utils.getLocksForTransaction(t1, transactionManager.getWaitForGraph());
                        List<Lock> waitLocksForSecondTransaction = Utils.getLocksForTransaction(t2, transactionManager.getWaitForGraph());
                        if (!waitLocksForFirstTransaction.isEmpty() && !waitLocksForSecondTransaction.isEmpty()) {
                            if (isTableLockedByTransaction(locksForFirstTransaction, waitLocksForSecondTransaction.get(0).getTable()) &&
                                    isTableLockedByTransaction(locksForSecondTransaction, waitLocksForFirstTransaction.get(0).getTable())) {
                                System.out.println("Deadlock detected between " + t1.toString() + " and " + t2.toString() + "!");
                                if (t1.getBeginTimestamp().before(t2.getBeginTimestamp())) {
                                    System.out.println(t2.toString() + " will be aborted!");
                                    TransactionManager.getInstance().abortTransaction(t2.getId());
                                } else {
                                    System.out.println(t1.toString() + " will be aborted!");
                                    TransactionManager.getInstance().abortTransaction(t1.getId());
                                }
                            }
                        }
                    }
                }
            }
            try {
                Thread.sleep(1000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    private boolean isTableLockedByTransaction(List<Lock> transactionLocks, String table) {
        for(Lock lock : transactionLocks)
            if (lock.getTable().equals(table))
                return true;
        return false;
    }

}
