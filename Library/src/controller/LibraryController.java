package controller;

import dao.BookStoreDAO;
import dao.UserDAO;
import model.Book;
import model.User;
import transaction.*;

import javax.sql.DataSource;
import java.sql.SQLException;
import java.util.List;

/**
 * Created by Elev on 05.02.2017.
 */
public class LibraryController {
    DataSource usersDataSource;
    DataSource booksDataSource;

    public LibraryController(DataSource usersDataSource, DataSource booksDataSource) {
        this.usersDataSource = usersDataSource;
        this.booksDataSource = booksDataSource;

        //create in memory tables
        UserDAO userDAO = new UserDAO();
        BookStoreDAO bookStoreDAO = new BookStoreDAO();
        try {
            userDAO.setConnection(usersDataSource.getConnection());
            bookStoreDAO.setConnection(booksDataSource.getConnection());
            userDAO.create();
            bookStoreDAO.create();
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
    }

    public List<User> getAllUsers(int transactionID) {
        UserDAO userDAO = new UserDAO();
        try {
            userDAO.setConnection(usersDataSource.getConnection());
        } catch (SQLException e) {
            System.out.println(e);
            return null;
        }
        TransactionManager.getInstance().acquireLock(Constants.USER_TABLE, Constants.READ, transactionID);
        return userDAO.getAllUsers();
    }

    public List<Book> getAllBooks(int transactionID) {
        BookStoreDAO bookStoreDAO = new BookStoreDAO();
        try {
            bookStoreDAO.setConnection(booksDataSource.getConnection());
        } catch (SQLException e) {
            System.out.println(e);
            return null;
        }

        TransactionManager.getInstance().acquireLock(Constants.BOOK_TABLE, Constants.READ, transactionID);
        return bookStoreDAO.getAllBooks();
    }

    public String startNewTransaction() {
        Transaction t = TransactionManager.getInstance().startNewTransaction();
        return t.toString();
    }

    public void commitTransaction(int id) {
        TransactionManager.getInstance().commitTransaction(id);
    }

    public void abortTransaction(int id) {
        TransactionManager.getInstance().abortTransaction(id);
    }

    public void addUser(User user, int transactionID) {
        UserDAO userDAO = new UserDAO();
        try {
            userDAO.setConnection(usersDataSource.getConnection());
        } catch (SQLException e) {
            System.out.println(e);
        }

        TransactionManager.getInstance().acquireLock(Constants.USER_TABLE, Constants.WRITE, transactionID);

        // store undo operation and perform DB operation
        final int userID = userDAO.insert(user);
        user.setId(userID);

        Operation insertUserRollback = userDAO.generateInsertOperationRollback(userID);
        Transaction transaction = Utils.getTransaction(transactionID, TransactionManager.getInstance().getTransactions());
        transaction.getOperations().push(insertUserRollback);
    }

    public void deleteUser(int userID, int transactionID) {
        UserDAO userDAO = new UserDAO();
        try {
            userDAO.setConnection(usersDataSource.getConnection());
        } catch (SQLException e) {
            System.out.println(e);
        }

        TransactionManager.getInstance().acquireLock(Constants.USER_TABLE, Constants.WRITE, transactionID);

        User user = userDAO.findUserByID(userID);
        userDAO.delete(userID);

        Operation deleteUserRollback = userDAO.generateDeleteOperationRollback(user);
        Transaction transaction = Utils.getTransaction(transactionID, TransactionManager.getInstance().getTransactions());
        transaction.getOperations().push(deleteUserRollback);
    }

    public void addBook(Book book, int transactionID) {
        BookStoreDAO bookStoreDAO = new BookStoreDAO();
        try {
            bookStoreDAO.setConnection(booksDataSource.getConnection());
        } catch (SQLException e) {
            System.out.println(e);
        }

        TransactionManager.getInstance().acquireLock(Constants.BOOK_TABLE, Constants.WRITE, transactionID);

        // store undo operation and perform DB operation
        final int bookID = bookStoreDAO.insert(book);
        book.setId(bookID);

        Operation insertBookRollback = bookStoreDAO.generateInsertOperationRollback(bookID);
        Transaction transaction = Utils.getTransaction(transactionID, TransactionManager.getInstance().getTransactions());
        transaction.getOperations().push(insertBookRollback);
    }

    public void updateBook(int bookID, int copies, int transactionID) {
        BookStoreDAO bookStoreDAO = new BookStoreDAO();
        try {
            bookStoreDAO.setConnection(booksDataSource.getConnection());
        } catch (SQLException e) {
            System.out.println(e);
        }


        TransactionManager.getInstance().acquireLock(Constants.BOOK_TABLE, Constants.WRITE, transactionID);

        Book oldBook = bookStoreDAO.findBookByID(bookID);

        bookStoreDAO.update(new Book(bookID, oldBook.getTitle(), oldBook.getPrice(), copies));

        Operation updateBookRollback = bookStoreDAO.generateUpdateOperationRollback(oldBook);
        Transaction transaction = Utils.getTransaction(transactionID, TransactionManager.getInstance().getTransactions());
        transaction.getOperations().push(updateBookRollback);

    }

    public void borrowBook(int userID, int bookID, int transactionID) {
        BookStoreDAO bookStoreDAO = new BookStoreDAO();

        try {
            bookStoreDAO.setConnection(booksDataSource.getConnection());
        } catch (SQLException e) {
            System.out.println(e);
        }

        TransactionManager.getInstance().acquireLock(Constants.BORROWED_BOOKS_TABLE, Constants.WRITE, transactionID);
        //perform operation
        Book book =  bookStoreDAO.findBookByID(bookID);
        if (book.getCopies() > 0) {
            int borrowedBookID = bookStoreDAO.borrowBook(userID, bookID);
            updateBook(bookID, book.getCopies() - 1, transactionID);


            Operation insertBorrowedBookRollback = bookStoreDAO.generateInsertBorrowedBookOperationRollback(borrowedBookID);
            Transaction transaction = Utils.getTransaction(transactionID, TransactionManager.getInstance().getTransactions());
            transaction.getOperations().push(insertBorrowedBookRollback);
        }

    }

    public void returnBook(int userID, int bookID, int transactionID) {
        BookStoreDAO bookStoreDAO = new BookStoreDAO();
        try {
            bookStoreDAO.setConnection(booksDataSource.getConnection());
        } catch (SQLException e) {
            System.out.println(e);
        }

        TransactionManager.getInstance().acquireLock(Constants.BORROWED_BOOKS_TABLE, Constants.WRITE, transactionID);
        //perform operation
        Book book =  bookStoreDAO.findBookByID(bookID);

        bookStoreDAO.returnBook(userID, bookID);
        updateBook(bookID, book.getCopies() + 1, transactionID);


        Operation returnBorrowedBookRollback = bookStoreDAO.generateReturnBorrowedBookOperationRollback(userID, bookID);
        Transaction transaction = Utils.getTransaction(transactionID, TransactionManager.getInstance().getTransactions());
        transaction.getOperations().push(returnBorrowedBookRollback);

    }

}
