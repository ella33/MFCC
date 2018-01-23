package transaction;

import java.sql.PreparedStatement;

public abstract class WriteOperation {
	
	protected PreparedStatement statement;
	
	public WriteOperation(PreparedStatement statement) {
		this.statement = statement;
	}
	
	public abstract void rollback();
	
	public String toString() {
		return statement.toString();
	}
}
