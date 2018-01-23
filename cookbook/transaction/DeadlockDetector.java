package transaction;

import java.util.ArrayList;
import java.util.List;

public class DeadlockDetector implements Runnable {

	private TransactionManager transactionManager;
	
	public DeadlockDetector(TransactionManager transactionManager) {
		this.transactionManager = transactionManager;
	}
	
	@Override
	public void run() {
		while(true){
			List<Transaction> transactionsCopy = new ArrayList<Transaction>(transactionManager.getTransactions());
			for(Transaction t1 : transactionsCopy){
				for(Transaction t2 : transactionsCopy){
					if(!t1.equals(t2)){
						if(getAllTablesATransactionHasLocked(t1).contains(getTheTableATransactionIsWaitingForToLock(t2)) &&
						   getAllTablesATransactionHasLocked(t2).contains(getTheTableATransactionIsWaitingForToLock(t1))){
							System.out.println("Deadlock detected between " + t1.toString() + " and " + t2.toString() + "!");
							if(t1.getBeginTimestamp().before(t2.getBeginTimestamp())){
								System.out.println(t2.toString() + " will be aborted!");
								TransactionManager.getInstance().abortTransaction(t2.getId());
							}
							else{
								System.out.println(t1.toString() + " will be aborted!");
								TransactionManager.getInstance().abortTransaction(t1.getId());
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
	
	private List<Table> getAllTablesATransactionHasLocked(Transaction transaction){
		List<Table> tables = new ArrayList<Table>();
		List<Lock> locksCopy = new ArrayList<Lock>(transactionManager.getLocks());
		for(Lock lock : locksCopy){
			if(lock.getTransaction().equals(transaction)){
				tables.add(lock.getTable());
			}
		}
		
		return tables;
	}
	
	private Table getTheTableATransactionIsWaitingForToLock(Transaction transaction){
		List<Wait> waitForGraphCopy = new ArrayList<Wait>(transactionManager.getWaitForGraph());
		for(Wait wait : waitForGraphCopy){
			if(wait.getTransaction().equals(transaction)){
				return wait.getTable();
			}
		}
		
		return null;
	}

}
