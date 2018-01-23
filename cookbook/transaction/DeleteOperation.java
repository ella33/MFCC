package transaction;

import java.sql.PreparedStatement;
import java.sql.SQLException;

public class DeleteOperation extends WriteOperation {

	public DeleteOperation(PreparedStatement statement) {
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
