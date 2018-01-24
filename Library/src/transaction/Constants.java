package transaction;

/**
 * Created by Elev on 05.02.2017.
 */
public class Constants {
    public static final int ACTIVE = 1;
    public static final int WAITING = 2;
    public static final int COMMITED = 3;
    public static final int ABORTED = 4;

    public static final String USER_TABLE = "user";
    public static final String BOOK_TABLE = "book";
    public static final String BORROWED_BOOKS_TABLE = "borrowed_book";

    public static final int READ = 0;
    public static final int WRITE = 1;
}
