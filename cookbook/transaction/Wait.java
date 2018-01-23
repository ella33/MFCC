package transaction;

public class Wait {

	private Transaction transaction;
	private Table table;
	private LockType lockType;
	
	public Wait() {
	}

	public Wait(Transaction transaction, Table table, LockType lockType) {
		this.transaction = transaction;
		this.table = table;
		this.lockType = lockType;
	}

	public Transaction getTransaction() {
		return transaction;
	}

	public void setTransaction(Transaction transaction) {
		this.transaction = transaction;
	}

	public Table getTable() {
		return table;
	}

	public void setTable(Table table) {
		this.table = table;
	}

	public LockType getLockType() {
		return lockType;
	}

	public void setLockType(LockType lockType) {
		this.lockType = lockType;
	}
	
	@Override
	public boolean equals(Object obj) {
		if(obj instanceof Wait){
			Wait other = (Wait) obj;
			return this.transaction.equals(other.getTransaction()) &&
				   this.table.equals(other.getTable()) &&
				   this.lockType.equals(other.lockType);
		}
		
		return true;
	}
	
}
