package com.mkyong;

import javax.activation.DataHandler;
import javax.activation.DataSource;
import javax.activation.FileDataSource;
import javax.mail.*;
import javax.mail.internet.InternetAddress;
import javax.mail.internet.MimeBodyPart;
import javax.mail.internet.MimeMessage;
import javax.mail.internet.MimeMultipart;
import java.io.*;
import java.util.Properties;

public class SendEmailSSL {

    public static void main(String[] args) throws IOException {

        final String username = "arlauskas.au@gmail.com";
        final String password = "lordijakas";

        Properties prop = new Properties();
        prop.put("mail.smtp.host", "smtp.gmail.com");
        prop.put("mail.smtp.port", "465");
        prop.put("mail.smtp.auth", "true");
        prop.put("mail.smtp.socketFactory.port", "465");
        prop.put("mail.smtp.socketFactory.class", "javax.net.ssl.SSLSocketFactory");

        Session session = Session.getInstance(prop,
                new javax.mail.Authenticator() {
                    protected PasswordAuthentication getPasswordAuthentication() {
                        return new PasswordAuthentication(username, password);
                    }
                });
        BufferedReader bufferedReader = new BufferedReader(new FileReader(new File("C:\\Users\\Aurimas\\Desktop\\end123.txt")));
        String email;
        int i = 0;

        while((email = bufferedReader.readLine()) != null && i <475)
        {
            if(email.equals("")) continue;
            try {
                i++;
                MimeBodyPart messageBodyPart = new MimeBodyPart();

                Message message = new MimeMessage(session);
                message.setFrom(new InternetAddress("from@gmail.com"));
                message.setRecipients(
                        Message.RecipientType.TO,
                        InternetAddress.parse(email)
                );
                message.setSubject("Applying for a summer job with J-1 visa");
                messageBodyPart.setText("Good afternoon,\n" +
                        "\n" +
                        "I am full - time student from Lithuania (European Union). I will participate in Summer Work and Travel USA Program (J-1 visa) and with this visa I will be able to work legally in the United States for four months, so I am really interested in working with You. The most suitable period for the employment would be from 1st of June till September 30th.\n" +
                        "\n" +
                        "I am responsible, active, punctual, polite, hard working, physically and mentally strong, also a quick learner. I am open to meet new people and learn more about other culture. I speak three languages ( my native - Lithuanian , English and a little bit of Russian). I have experience working in a big group as well as independently, so a job requiring team-work or individual skills would not be a difficulty.\n" +
                        "\n" +
                        "I will be arriving to USA together with my friend, Greta. She, just like me, is participating in the program. If you have job openings for more than one person, we will be more than happy to work.\n" +
                        "\n" +
                        "My resume is added for your review. If more information is needed, please feel free to contact me.\n" +
                        "Thank you for your time and consideration.\n" +
                        "\n" +
                        "I am looking forward to hearing from You,\n" +
                        "\n"
                        +"With best regards." +
                        "\n" +
                        "Aurimas Arlauskas" + "\n" + "https://drive.google.com/file/d/15Os08jYFAYrx4HrDDlkYZDlOzFx3B_9D/view?usp=sharing");

                Multipart multipart = new MimeMultipart();

                multipart.addBodyPart(messageBodyPart);
                messageBodyPart = new MimeBodyPart();
                DataSource source = new FileDataSource("C:\\Users\\Aurimas\\Desktop\\video.mp4");
                messageBodyPart.setDataHandler(new DataHandler(source));
                messageBodyPart.setFileName("Greta Minelgaite Video");
                multipart.addBodyPart(messageBodyPart);
                message.setContent(multipart);



                Transport.send(message);

                System.out.println(i + " " + email);

            } catch (MessagingException e) {
                System.out.println(i + " " + email + e.toString());
            }
        }
        }



}