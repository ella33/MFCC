package transaction;

import java.sql.Timestamp;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

/**
 * Created by Elev on 05.02.2017.
 */
public class TransactionManager {

    private static TransactionManager transactionManager;
    private List<Transaction> transactions;
    private int transactionId;

    private List<Lock> locks;
    private List<Lock> waitForGraph;

    private TransactionManager() {
        transactionId = 0;
        transactions = new ArrayList<>();
        locks = new ArrayList<>();
        waitForGraph = new ArrayList<>();

        DeadlockDetector deadlockDetector = new DeadlockDetector(this);
        Thread deadlockDetectorThread = new Thread(deadlockDetector);
        deadlockDetectorThread.start();
    }

    public static TransactionManager getInstance(){
        if(transactionManager == null){
            transactionManager = new TransactionManager();
        }

        return transactionManager;
    }

    public Transaction startNewTransaction(){
        Transaction transaction = new Transaction(transactionId++, new Timestamp(System.currentTimeMillis()), Constants.ACTIVE);
        transactions.add(transaction);
        return transaction;
    }

    public  void commitTransaction(int id){
        Transaction transaction = Utils.getTransaction(id, transactions);
        releaseLocks(id);
        transaction.setStatus(Constants.COMMITED);
    }

    public void abortTransaction(int id){
        Transaction transaction = Utils.getTransaction(id, transactions);
        rollbackOperations(transaction);
        releaseLocks(id);
        transaction.setStatus(Constants.ABORTED);
    }

    public  void acquireLock(String table, int lockType, int transactionId) {
        Transaction transaction = Utils.getTransaction(transactionId, transactions);
        List<Lock> locksForTable = Utils.getLocksForTable(table, locks);

        if (locksForTable.isEmpty()) {
            synchronized (locks) {
                locks.add(new Lock(transaction, table, lockType));
            }
        } else if (locksForTable.size() == 1 && locksForTable.iterator().next().getTransaction().equals(transaction)) {
            if (lockType == Constants.WRITE) {
                synchronized (locks) {
                    locksForTable.iterator().next().setLockType(lockType);
                }
            }
        } else {
            if (lockType == Constants.READ) {
                boolean sharedLock = true;
                for (Lock lockForTable : locksForTable) {
                    if (lockForTable.getLockType() == Constants.WRITE) {
                        sharedLock = false;
                        break;
                    }
                }
                if (sharedLock) {
                    synchronized (locks) {
                        locks.add(new Lock(transaction, table, lockType));
                    }
                    return;
                }
            }
            Lock waitLock = new Lock(transaction, table, lockType);
            waitForLock(waitLock);
            while (transaction.getStatus() == Constants.WAITING) {
                try {
                    System.out.println("Transaction ID = " + transaction.getId() + " waiting to aquire lock...");
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
            if (transaction.getStatus() == Constants.ACTIVE) {
                System.out.println("Transaction ID = " + transaction.getId() + " has the lock!");
                deleteWaitForLock(waitLock);
                locksForTable = Utils.getLocksForTable(table, locks);
                synchronized (locks) {
                    if (locksForTable.isEmpty()) {
                        locks.add(new Lock(transaction, table, lockType));
                    } else {
                        locksForTable.iterator().next().setLockType(lockType);
                    }
                }
            }
        }

    }

    public  void releaseLocks(int transactionId) {
        List<String> deletedLocksFromTable = new ArrayList<>();

        Iterator itr = locks.iterator();
        while (itr.hasNext()) {
            Lock nextLock = (Lock) itr.next();
            if (nextLock.getTransaction().getId() == transactionId) {
                if (!deletedLocksFromTable.contains(nextLock.getTable())) {
                    deletedLocksFromTable.add(nextLock.getTable());
                }
                synchronized (locks) {
                    itr.remove();
                }
            }
        }
        notifyWaiters(deletedLocksFromTable);
    }

    public void notifyWaiters(List<String> deletedLocksFromTable) {
        for (String table : deletedLocksFromTable){
            List<Lock> waitsForTable = Utils.getLocksForTable(table, waitForGraph);
            List<Lock> locksForTable = Utils.getLocksForTable(table, locks);
            if(locksForTable.isEmpty() && !waitsForTable.isEmpty()) {
                Lock waitLock = waitsForTable.get(0);
                Transaction transaction = waitLock.getTransaction();

                if(waitLock.getLockType() == Constants.READ){
                    transaction.setStatus(Constants.ACTIVE);
                    for(int i = 1; i < waitsForTable.size(); i++){
                        Lock nextWaitLock = waitsForTable.get(i);
                        if (nextWaitLock.getLockType() == Constants.READ) {
                            nextWaitLock.getTransaction().setStatus(Constants.ACTIVE);
                        }

                    }
                }
                else{
                    transaction.setStatus(Constants.ACTIVE);
                }
            }
            else if(!locksForTable.isEmpty()){
                if(locksForTable.size() == 1){
                    Transaction transactionLocking = locksForTable.get(0).getTransaction();
                    List<Lock> locksForTransaction = Utils.getLocksForTransaction(transactionLocking, waitsForTable);
                    if(!locksForTransaction.isEmpty()){
                        for (Lock lockPerTransaction: locksForTransaction)
                            lockPerTransaction.getTransaction().setStatus(Constants.ACTIVE);
                    }
                } else {
                    boolean sharedLock = true;
                    for(Lock lock: locksForTable) {
                        if (lock.getLockType() == Constants.WRITE) {
                            sharedLock = false;
                            break;
                        }
                    }
                    if (sharedLock) {
                        for(int i = 0; i < waitsForTable.size(); i++){
                            Lock nextWaitLock = waitsForTable.get(i);
                            if (nextWaitLock.getLockType() == Constants.READ) {
                                nextWaitLock.getTransaction().setStatus(Constants.ACTIVE);
                            }

                        }
                    }

                }
            }
        }
    }

    public void rollbackOperations(Transaction transaction) {
        for(Operation operation : transaction.getOperations()){
            operation.rollback();
            System.out.println("Executed rollback operation for " + transaction.toString() + ": " + operation.toString());
        }
    }


    public  void waitForLock(Lock waitLock){
        Transaction lockTransaction = waitLock.getTransaction();
        lockTransaction.setStatus(Constants.WAITING);
        synchronized (waitForGraph) {
            waitForGraph.add(waitLock);
        }
    }

    public void deleteWaitForLock(Lock waitLock){
        synchronized (waitForGraph) {
            waitForGraph.remove(waitLock);
        }
    }

    public List<Transaction> getTransactions() {
        return transactions;
    }

    public List<Lock> getWaitForGraph() {
        return waitForGraph;
    }

    public List<Lock> getLocks() {
        return locks;
    }
}
