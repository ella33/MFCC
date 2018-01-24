package servlet;

import controller.LibraryController;
import dao.UserDAO;
import model.Book;
import model.User;

import javax.naming.Context;
import javax.naming.InitialContext;
import javax.naming.NamingException;
import javax.servlet.ServletConfig;
import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.sql.DataSource;
import java.io.IOException;
import java.io.PrintWriter;
import java.sql.SQLException;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;

/**
 * Created by Elev on 04.02.2017.
 */
@WebServlet(name = "Library",
            urlPatterns = "/library")
public class Library extends HttpServlet {

    private static final String START_TRANSACTION = "startTransaction";
    private static final String COMMIT_TRANSACTION = "commitTransaction";
    private static final String ABORT_TRANSACTION = "abortTransaction";
    private static final String LIST_ALL_USERS = "listUsers";
    private static final String LIST_ALL_BOOKS = "listBooks";
    private static final String BUY_BOOK = "buyBook";
    private static final String BORROW_BOOK = "borrowBook";
    private static final String ADD_BOOK = "addBook";
    private static final String RETURN_BOOK = "returnBook";
    private static final String ADD_USER = "addUser";
    private static final String DELETE_USER = "deleteUser";

    LibraryController libraryController;

    @Override
    public void init(ServletConfig config) throws ServletException {
        try {
            Context initContext = new InitialContext();
            Context envContext = (Context) initContext.lookup("java:comp/env");
            DataSource usersDataSource = (DataSource) envContext.lookup("jdbc/LibraryUsersDB");
            DataSource booksDataSource = (DataSource) envContext.lookup("jdbc/LibraryStoreDB");
            libraryController = new LibraryController(usersDataSource, booksDataSource);

        } catch (NamingException ex) {
            System.out.println(ex);
        }
    }



    protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
        doGet(request, response);
    }

    protected void doGet(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
    	System.out.println("HELLO");
        if (request.getParameter(LIST_ALL_BOOKS) != null) {
            System.out.println("Listing all books");
            int id = Integer.parseInt(request.getParameter("transactionID"));
            List<Book> books = libraryController.getAllBooks(id);
            request.setAttribute("books", books);
            request.getRequestDispatcher("/books.jsp").forward(request, response);
        } else if (request.getParameter(LIST_ALL_USERS) != null){
            System.out.println("Listing all users");
            int id = Integer.parseInt(request.getParameter("transactionID"));
            List<User> users = libraryController.getAllUsers(id);
            request.setAttribute("users", users);
            request.getRequestDispatcher("/users.jsp").forward(request, response);
        } else if (request.getParameter(START_TRANSACTION) != null) {
            System.out.println("Started new transaction");
            displayString(request, response, "Started new transaction", libraryController.startNewTransaction());
        } else if (request.getParameter(COMMIT_TRANSACTION) != null) {
            System.out.println("Commit transaction");
            int id = Integer.parseInt(request.getParameter("transactionID"));
            libraryController.commitTransaction(id);
            displayString(request, response, "Transaction was commited successfully", "");
        } else if (request.getParameter(ABORT_TRANSACTION) != null) {
            System.out.println("Abort transaction");
            int id = Integer.parseInt(request.getParameter("transactionID"));
            libraryController.abortTransaction(id);
            displayString(request, response, "Transaction was aborted successfully", "");
        } else if (request.getParameter(ADD_USER) != null){
            System.out.println("Add a new user");
            addUser(request, response);
        } else if (request.getParameter(DELETE_USER) != null){
            System.out.println("Delete user");
            deleteUser(request, response);
        } else if (request.getParameter(ADD_BOOK) != null) {
            System.out.println("Add new book");
            addBook(request, response);
        } else if (request.getParameter(BUY_BOOK) != null) {
            System.out.println("Buy book");
            buyBook(request, response);
        } else if (request.getParameter(BORROW_BOOK) != null) {
            System.out.println("Borrow book");
            borrowBook(request, response);
        } else if (request.getParameter(RETURN_BOOK) != null) {
            System.out.println("Return book");
            returnBook(request, response);
        }
        else {
            request.getRequestDispatcher("/index.jsp").forward(request, response);
        }

    }

    private void displayString(HttpServletRequest request, HttpServletResponse response, String message, String value)
            throws ServletException, IOException {
        PrintWriter out = response.getWriter();
        out.println(message);
        out.println(value);
        out.close();
    }

    private void addUser(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
        String username = request.getParameter("username");
        int transactionID = Integer.parseInt(request.getParameter("transactionID"));

        User user = new User();
        user.setUsername(username);
        libraryController.addUser(user, transactionID);
        displayString(request, response, "User was added successfully", "");
    }

    private void deleteUser(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
        int userID = Integer.parseInt(request.getParameter("userID"));
        int transactionID = Integer.parseInt(request.getParameter("transactionID"));

        libraryController.deleteUser(userID, transactionID);
        displayString(request, response, "User was deleted successfully", "");
    }

    private void addBook(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
        String title = request.getParameter("title");
        int price = Integer.parseInt(request.getParameter("price"));
        int copies = Integer.parseInt(request.getParameter("copies"));
        int transactionID = Integer.parseInt(request.getParameter("transactionID"));

        Book book = new Book();
        book.setTitle(title);
        book.setPrice(price);
        book.setCopies(copies);
        libraryController.addBook(book, transactionID);
        displayString(request, response, "Book was added successfully", "");
    }

    private void buyBook(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
        int bookID = Integer.parseInt(request.getParameter("bookIDUpdate"));
        int copies = Integer.parseInt(request.getParameter("copiesUpdate"));
        int transactionID = Integer.parseInt(request.getParameter("transactionID"));

        libraryController.updateBook(bookID, copies, transactionID);
        displayString(request, response, "Book was updated successfully", "");
    }

    private void borrowBook(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
        int userID = Integer.parseInt(request.getParameter("userIDBorrow"));
        int bookID = Integer.parseInt(request.getParameter("bookIDBorrow"));
        int transactionID = Integer.parseInt(request.getParameter("transactionID"));

        libraryController.borrowBook(userID, bookID, transactionID);
        displayString(request, response, "Book was borrowed successfully", "");
    }

    private void returnBook(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
        int userID = Integer.parseInt(request.getParameter("userID"));
        int bookID = Integer.parseInt(request.getParameter("bookID"));
        int transactionID = Integer.parseInt(request.getParameter("transactionID"));

        libraryController.returnBook(userID, bookID, transactionID);
        displayString(request, response, "Book was borrowed successfully", "");
    }
}
