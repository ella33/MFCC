package transaction;

import java.sql.PreparedStatement;
import java.sql.SQLException;

public class UpdateOperation extends WriteOperation {

	public UpdateOperation(PreparedStatement statement) {
		super(statement);
	}
	
	@Override
	public void rollback() {
		try {
			statement.executeUpdate();
		} catch (SQLException e) {
			System.err.println();
		}
	}

}
