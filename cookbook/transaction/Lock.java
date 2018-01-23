package transaction;

public class Lock {

	private Transaction transaction;
	private Table table;
	private LockType type;
	
	public Lock() {
	}

	public Lock(Transaction transaction, Table table, LockType type) {
		this.transaction = transaction;
		this.table = table;
		this.type = type;
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

	public LockType getType() {
		return type;
	}

	public void setType(LockType type) {
		this.type = type;
	}
	
}
