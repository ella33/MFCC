package transaction;

/**
 * Created by Elev on 05.02.2017.
 */
import java.sql.PreparedStatement;
import java.sql.SQLException;

public  class Operation {

    protected PreparedStatement statement;

    public Operation(PreparedStatement statement) {
        this.statement = statement;
    }

    public void rollback() {
        try {
            statement.executeUpdate();
        } catch (SQLException e) {
            System.err.println();
        }
    }

    public String toString() {
        return statement.toString();
    }
}
