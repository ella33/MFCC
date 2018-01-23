package transaction;

import java.sql.Timestamp;
import java.util.Stack;

public class Transaction {
	
	private int id;
	private Timestamp beginTimestamp;
	private TransactionStatus status;
	
	private Stack<WriteOperation> writeOperations;
	
	public Transaction() {
	}

	public Transaction(int id, Timestamp beginTimestamp,
			TransactionStatus status) {
		this.id = id;
		this.beginTimestamp = beginTimestamp;
		this.status = status;
		this.writeOperations = new Stack<WriteOperation>();
	}

	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public Timestamp getBeginTimestamp() {
		return beginTimestamp;
	}

	public void setBeginTimestamp(Timestamp beginTimestamp) {
		this.beginTimestamp = beginTimestamp;
	}

	public TransactionStatus getStatus() {
		return status;
	}

	public void setStatus(TransactionStatus status) {
		this.status = status;
	}
	
	public Stack<WriteOperation> getWriteOperations() {
		return writeOperations;
	}
	
	@Override
	public boolean equals(Object obj) {
		if(obj instanceof Transaction){
			Transaction other = (Transaction) obj;
			
			return this.id == other.getId();
		}
		
		return false;
	}
	
	@Override
	public String toString() {
		return "Transaction ID = " + id;
	}
	
}
