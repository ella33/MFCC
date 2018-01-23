package transaction;

import java.sql.Timestamp;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

public class TransactionManager {

	private static TransactionManager transactionManager;
	
	private int transactionId;
	private List<Lock> locks;
	private List<Transaction> transactions;
	private List<Wait> waitForGraph;
	
	private TransactionManager() {
		transactionId = 0;
		locks = new ArrayList<Lock>();
		transactions = new ArrayList<Transaction>();
		waitForGraph = new ArrayList<Wait>();
		
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
	
	public synchronized Transaction startNewTransaction(){
		Transaction transaction = new Transaction(transactionId++, new Timestamp(System.currentTimeMillis()), TransactionStatus.ACTIVE);
		transactions.add(transaction);
		return transaction;
	}
	
	public synchronized void commitTransaction(int id){
		Transaction transaction = getTransactionById(id);
		// delete from locks where transaction.id = id
		List<Lock> deletedLocks = new ArrayList<Lock>();
		for (Iterator<Lock> iterator = locks.iterator(); iterator.hasNext();) {
		    Lock lock = iterator.next();
		    if (lock.getTransaction().getId() == id) {
		    	deletedLocks.add(lock);
		        iterator.remove();
		    }
		}
		
		// each time locks get deleted, call the corresponding trigger
		if(!deletedLocks.isEmpty()){
			triggerLocksDeleted(deletedLocks);
		}
		
		transaction.setStatus(TransactionStatus.COMMITED);
	}
	
	public synchronized void abortTransaction(int id){
		Transaction transaction = getTransactionById(id);
		
		// rollback
		for(WriteOperation writeOperation : transaction.getWriteOperations()){
			writeOperation.rollback();
			System.out.println("Executed rollback operation for " + transaction.toString() + ": " + writeOperation.toString());
		}
		
		// delete from locks where transaction.id = id
		List<Lock> deletedLocks = new ArrayList<Lock>();
		for (Iterator<Lock> iterator = locks.iterator(); iterator.hasNext();) {
		    Lock lock = iterator.next();
		    if (lock.getTransaction().getId() == id) {
		    	deletedLocks.add(lock);
		        iterator.remove();
		    }
		}
		
		// each time locks get deleted, call the corresponding trigger
		if(!deletedLocks.isEmpty()){
			triggerLocksDeleted(deletedLocks);
		}
		
		transaction.setStatus(TransactionStatus.ABORTED);
	}
	
	public Transaction getTransactionById(int id){
		for(Transaction transaction : transactions){
			if(transaction.getId() == id){
				return transaction;
			}
		}
		
		return null;
	}
	
	public List<Lock> getLocksForTable(Table table){
		List<Lock> locksForTable = new ArrayList<Lock>();
		for(Lock lock : locks){
			if(lock.getTable().equals(table)){
				locksForTable.add(lock);
			}
		}
		
		return locksForTable;
	}
	
	public synchronized void setWriteLock(Transaction transaction, Table table){
		locks.add(new Lock(transaction, table, LockType.WRITE));
	}
	
	public synchronized void setReadLock(Transaction transaction, Table table){
		locks.add(new Lock(transaction, table, LockType.READ));
	}
	
	public synchronized Wait waitForWriteLock(Transaction transaction, Table table){
		transaction.setStatus(TransactionStatus.WAITING);
		Wait wait = new Wait(transaction, table, LockType.WRITE);
		waitForGraph.add(wait);
		
		return wait;
	}
	
	public synchronized void deleteWaitForLock(Wait wait){
		waitForGraph.remove(wait);
	}
	
	public synchronized Wait waitForReadLock(Transaction transaction, Table table){
		transaction.setStatus(TransactionStatus.WAITING);
		Wait wait = new Wait(transaction, table, LockType.READ);
		waitForGraph.add(wait);
		
		return wait;
	}
	
	private void triggerLocksDeleted(List<Lock> deletedLocks){
		for(Lock lock : deletedLocks){
			Table table = lock.getTable();
			// if the table is not locked anymore and there are other transactions waiting to r/w lock the table
			if(!isTableLocked(table) && areThereTransactionsWaitingForTable(table)){
				List<Wait> waitsForTable = getAllWaitsForTable(table);
				Wait wait = waitsForTable.get(0);
				Transaction transaction = wait.getTransaction();
				// if the transaction is waiting to read lock => set transaction active and set all other transactions active waiting to
				//												 read lock the same resource
				if(wait.getLockType().equals(LockType.READ)){
					transaction.setStatus(TransactionStatus.ACTIVE);
					for(int i = 1; i < waitsForTable.size(); i++){
						waitsForTable.get(i).getTransaction().setStatus(TransactionStatus.ACTIVE);
					}
				}
				else{
					transaction.setStatus(TransactionStatus.ACTIVE);
				}
			}
			else if(isTableLocked(table)){
				List<Lock> locksForTable = getLocksForTable(table);
				if(locksForTable.size() == 1){
					// if there is only one transaction locking the table (it can only be a shared lock)
					Transaction transactionReadLocking = locksForTable.iterator().next().getTransaction();
					if(isTransactionWaitingToLockTable(transactionReadLocking, table)){
						// and it's also waiting for exclusive lock => the transaction has a read lock and wants a write lock =>
						// => transaction becomes active (because it's the only one having a read lock on the table)
						transactionReadLocking.setStatus(TransactionStatus.ACTIVE);
					}
				}
			}
		}
	}
	
	private List<Wait> getAllWaitsForTable(Table table){
		List<Wait> waits = new ArrayList<Wait>();
		for(Wait wait : waitForGraph){
			if(wait.getTable().equals(table)){
				waits.add(wait);
			}
		}
		
		return waits;
	}
	
	private boolean isTableLocked(Table table){
		return !getLocksForTable(table).isEmpty();
	}
	
	private boolean areThereTransactionsWaitingForTable(Table table){
		for(Wait wait : waitForGraph){
			if(wait.getTable().equals(table)){
				return true;
			}
		}
		
		return false;
	}
	
	private boolean isTransactionWaitingToLockTable(Transaction transaction, Table table){
		for(Wait wait : waitForGraph){
			if(wait.getTransaction().equals(transaction) && wait.getTable().equals(table)){
				return true;
			}
		}
		
		return false;
	}
	
	public List<Lock> getLocks() {
		return locks;
	}

	public void setLocks(List<Lock> locks) {
		this.locks = locks;
	}

	public List<Transaction> getTransactions() {
		return transactions;
	}

	public void setTransactions(List<Transaction> transactions) {
		this.transactions = transactions;
	}

	public List<Wait> getWaitForGraph() {
		return waitForGraph;
	}

	public void setWaitForGraph(List<Wait> waitForGraph) {
		this.waitForGraph = waitForGraph;
	}
	
}
